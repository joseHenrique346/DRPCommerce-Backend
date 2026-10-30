using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Orders;

/// <summary>
/// Finaliza a compra dos itens selecionados do carrinho: recalcula tudo com os dados atuais, reserva o estoque até o prazo
/// de pagamento, cria o pedido pendente com a cobrança e tira do carrinho os itens comprados — tudo em uma transação.
/// A cobrança no gateway é criada em seguida; repetir o checkout com a mesma chave devolve o mesmo pedido e recria
/// a cobrança se ela ainda não existir.
/// </summary>
public sealed class CheckoutService(
    IOrderRepository orders,
    IRepository<Transaction> transactions,
    ICartRepository carts,
    ICustomerAddressRepository addresses,
    IRepository<Customer> customers,
    ICouponRepository coupons,
    IInventoryRepository inventory,
    IProductRepository products,
    CouponEligibilityService eligibility,
    CategoryCatalogReader categoryReader,
    StoreSettingsReader storeSettings,
    OrderLifecycleService lifecycle,
    IPaymentGateway gateway,
    OrderWorkflowScope scope,
    IUnitOfWork unitOfWork,
    ICurrentIdentity actor,
    ITenantProvider tenantProvider,
    TimeProvider time)
{
    public async Task<OrderDetailResponse> PlaceOrderAsync(CheckoutRequest request, CancellationToken ct)
    {
        var customerId = actor.CustomerId;
        var enterpriseId = tenantProvider.GetEnterpriseId();
        var key = request.IdempotencyKey.Trim();

        if (await orders.FindByIdempotencyKeyAsync(enterpriseId, customerId, key, ct) is { } previous)
            return await CompleteAsync(enterpriseId, customerId, previous.Id, ct);

        var categories = await categoryReader.ReadAsync(storefront: true, ct);

        if (categories.IsFailure)
            throw new SecurityFailure(404, "Store.Unavailable", "Loja indisponível.");

        var customer = await customers.FindAsync(item => item.Id == customerId && item.IsActive, ct, tracking: false)
            ?? throw new SecurityFailure(401, "Identity.InvalidCredentials", "Credenciais inválidas ou acesso temporariamente indisponível.");
        var settings = await storeSettings.GetCurrentAsync(ct);
        var visible = categories.Content!.Select(category => category.Id).ToHashSet();
        var now = time.GetUtcNow().UtcDateTime;
        long orderId;

        await using (var transaction = await unitOfWork.BeginTransactionAsync(ct))
        {
            await products.LockCatalogAsync(enterpriseId, ct);

            // Duas tentativas simultâneas com a mesma chave: a segunda espera o bloqueio e devolve o pedido da primeira.
            if (await orders.FindByIdempotencyKeyAsync(enterpriseId, customerId, key, ct) is { } concurrent)
            {
                await transaction.CommitAsync(ct);

                return await CompleteAsync(enterpriseId, customerId, concurrent.Id, ct);
            }

            var cart = await carts.GetForCustomerAsync(enterpriseId, customerId, tracking: true, ct);
            var selected = cart?.ListCartItem.Where(item => item.IsSelected).OrderBy(item => item.Id).ToList() ?? [];

            if (selected.Count == 0)
                throw new SecurityFailure(400, "Checkout.NoItemsSelected", "Selecione ao menos um item do carrinho para finalizar a compra.");

            var unavailable = selected.Where(item => item.GetAvailability(settings.AllowBackorder, visible) != CartItemAvailability.Available).ToList();

            if (unavailable.Count > 0)
                throw new SecurityFailure(409, "Checkout.ItemsUnavailable",
                    $"Itens indisponíveis: {string.Join(", ", unavailable.Select(item => item.Product.Name))}. Atualize o carrinho.");

            var quantity = selected.Sum(item => item.Quantity);

            if (!settings.IsWithinItemLimit(quantity))
                throw new SecurityFailure(400, "Checkout.MaxItemsExceeded", $"O pedido excede o limite de {settings.MaxItemsPerOrder} itens da loja.");

            var (addressLine, city, stateId, zipCode) = await ResolveAddressAsync(request, customer, ct);

            var expiresAt = now.AddMinutes(settings.PendingOrderExpirationMinutes);
            var order = Order.CreateCheckout(enterpriseId, customerId, addressLine, city, stateId, zipCode, request.Notes?.Trim() ?? string.Empty,
                key, request.PaymentMethodId, expiresAt, now);
            order.AssignNumber(await orders.GetNextOrderNumberAsync(enterpriseId, ct));

            foreach (var item in selected)
                order.AddProductItem(item.Product, item.ProductVariant, item.Quantity, settings);

            await ApplyCouponAsync(order, request.CouponCode, customerId, settings, now, ct);

            if (!settings.MeetsMinOrderValue(order.SubTotal))
                throw new SecurityFailure(400, "Checkout.MinOrderValue", $"O valor mínimo para comprar nesta loja é {settings.MinOrderValue:0.00}.");

            ReserveStock(order, selected, settings, key, expiresAt, now);
            transactions.Add(Transaction.CreatePendingPayment(order, request.PaymentMethodId, gateway.Provider));
            order.LockForPayment();

            foreach (var item in selected)
                carts.RemoveItem(cart!.RemoveItem(item.Id));

            await orders.AddAsync(order, ct);

            using (scope.Begin(enterpriseId, customerId))
                await unitOfWork.CommitAsync(ct);

            await transaction.CommitAsync(ct);
            orderId = order.Id;
        }

        return await CompleteAsync(enterpriseId, customerId, orderId, ct);
    }

    private async Task<(string AddressLine, string City, long StateId, string ZipCode)> ResolveAddressAsync(CheckoutRequest request, Customer customer,
        CancellationToken ct)
    {
        if (!request.AddressId.HasValue && !request.UseProfileAddress)
            return (request.ShippingAddressLine!, request.ShippingCity!, request.ShippingStateId!.Value, request.ShippingZipCode!);

        var saved = await addresses.ListForCustomerAsync(customer.EnterpriseId, customer.Id, tracking: false, ct);
        var address = request.AddressId.HasValue
            ? saved.FirstOrDefault(item => item.Id == request.AddressId)
                ?? throw new SecurityFailure(404, "CustomerAddress.NotFound", "Endereço não encontrado.")
            : saved.FirstOrDefault(item => item.IsDefault);

        if (address is not null)
            return (address.AddressLine, address.City, address.StateId, address.ZipCode);

        if (string.IsNullOrWhiteSpace(customer.AddressLine) || string.IsNullOrWhiteSpace(customer.City) || string.IsNullOrWhiteSpace(customer.ZipCode))
            throw new SecurityFailure(400, "Checkout.AddressRequired", "Cadastre um endereço de entrega para finalizar a compra.");

        return (customer.AddressLine, customer.City, customer.StateId, customer.ZipCode);
    }

    private async Task ApplyCouponAsync(Order order, string? couponCode, long customerId, StoreSettings settings, DateTime now, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return;

        var coupon = await coupons.GetByCodeAsync(couponCode, ct)
            ?? throw new SecurityFailure(404, "Coupon.NotFound", "Cupom não encontrado.");
        var history = await eligibility.GetHistoryAsync(coupon.Id, customerId, null, ct);
        var evaluation = coupon.Evaluate(now, order.SubTotal, order.TotalQuantity, history);

        if (!evaluation.IsValid)
            throw new SecurityFailure(400, evaluation.ErrorCode!, evaluation.Message!);

        order.ApplyCoupon(coupon, history, now, settings);
    }

    private void ReserveStock(Order order, List<CartItem> selected, StoreSettings settings, string key, DateTime expiresAt, DateTime now)
    {
        var tracked = selected.Where(item => OrderLifecycleService.ControlsStock(item.ProductVariant)).ToList();

        if (tracked.Count == 0)
            return;

        var reservation = StockReservation.Create(order.EnterpriseId, $"checkout-{order.CustomerId}-{key}", $"Checkout do cliente {order.CustomerId}", expiresAt);

        foreach (var item in tracked)
        {
            var variant = item.ProductVariant;

            if (!settings.AllowBackorder && variant.AvailableQuantity < item.Quantity)
                throw new SecurityFailure(409, "Checkout.ItemsUnavailable", $"Estoque insuficiente para {item.Product.Name}. Atualize o carrinho.");

            reservation.AddLine(variant, item.Quantity, settings.AllowBackorder);
            variant.ReserveStock(item.Quantity, settings.AllowBackorder);
            inventory.AddMovement(StockMovement.Create(variant, reservation, StockMovementType.Reservation, reservation.IdempotencyKey,
                0, item.Quantity, "Reserva do checkout", null, now));
        }

        inventory.AddReservation(reservation);
        order.AttachStockReservation(reservation);
    }

    /// <summary>
    /// Garante a cobrança do pedido: pedidos sem valor a pagar (ex.: cupom de 100%) são confirmados na hora;
    /// os demais recebem a cobrança do gateway, criada uma única vez.
    /// </summary>
    private async Task<OrderDetailResponse> CompleteAsync(long enterpriseId, long customerId, long orderId, CancellationToken ct)
    {
        var order = await orders.GetForWorkflowAsync(enterpriseId, orderId, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        if (order.IsAwaitingPayment && order.TotalAmount == 0)
        {
            await lifecycle.ConfirmPaymentAsync(enterpriseId, orderId, OrderChangeSource.System, null, null, "sem-cobranca", 0,
                "Pedido sem valor a pagar", ct);
        }
        else if (order.IsAwaitingPayment && order.ListTransaction.FirstOrDefault(item => item.IsPendingPayment) is { } payment &&
            string.IsNullOrEmpty(payment.GatewayReference))
        {
            var customer = await customers.FindAsync(item => item.Id == customerId, ct, tracking: false);
            var settings = await storeSettings.GetForEnterpriseAsync(enterpriseId, ct);
            var session = await gateway.CreatePaymentAsync(new PaymentRequest(enterpriseId, order.Id, customerId, customer?.FullName ?? string.Empty,
                customer?.Email.Value ?? string.Empty, payment.Amount, settings.CurrencyCode, payment.TransactionMethodId,
                order.PaymentExpiresAt ?? time.GetUtcNow().UtcDateTime, $"order-{order.Id}"), ct);

            payment.AttachGatewayPayment(session.Reference, new PaymentSessionPayload(session.CheckoutUrl, session.Instructions, session.ExpiresAt).Serialize());

            using (scope.Begin(enterpriseId, customerId))
                await unitOfWork.CommitAsync(ct);
        }

        var detail = await orders.GetDetailAsync(orderId, customerId, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        return OrderDetailResponse.FromEntity(detail, backoffice: false);
    }
}
