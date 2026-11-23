using MediatR;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Enums.Inventory;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Inventory;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orders,
    IRepository<Transaction> transactions,
    ICartRepository carts,
    ICustomerAddressRepository addresses,
    ICustomerRepository customers,
    ICouponRepository coupons,
    IRepository<CouponRedemption> redemptions,
    IInventoryRepository inventory,
    IProductRepository products,
    CouponCustomerHistoryReader eligibility,
    CategoryCatalogReader categoryReader,
    StoreSettingsReader storeSettings,
    IPaymentGatewayRegistry gateways,
    IPaymentAttemptStore paymentAttempts,
    OrderWorkflowScope scope,
    IUnitOfWork unitOfWork,
    ICurrentIdentity actor,
    ITenantProvider tenantProvider,
    TimeProvider time,
    OrderStockWriter stock,
    OrderCouponRedemptionWriter couponUses,
    ISender sender)
    : WorkflowRequestHandler<PlaceOrderCommand, OrderDetailResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<OrderDetailResponse> ExecuteAsync(PlaceOrderCommand command, CancellationToken ct)
    {
        var request = command.Request;
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
            await customers.LockAsync(customerId, ct);
            customer = await customers.FindAsync(Customer => Customer.Id == customerId && Customer.EnterpriseId == enterpriseId &&
                Customer.IsActive && !Customer.IsDeleted && Customer.PersonalDataErasedAt == null, ct, tracking: false)
                ?? throw new SecurityFailure(401, "Identity.InvalidCredentials", "A conta não está disponível para novas compras.");

            // Duas tentativas simultâneas com a mesma chave: a segunda espera o bloqueio e devolve o pedido da primeira.
            if (await orders.FindByIdempotencyKeyAsync(enterpriseId, customerId, key, ct) is { } concurrent)
            {
                await transaction.CommitAsync(ct);
                // Remove a transação atual do DbContext antes do processamento independente do pagamento.
                await transaction.DisposeAsync();

                return await CompleteAsync(enterpriseId, customerId, concurrent.Id, ct);
            }

            var cart = await carts.GetForCustomerAsync(enterpriseId, customerId, tracking: true, ct);
            var selected = cart?.ListCartItem.Where(item => item.IsSelected).OrderBy(item => item.Id).ToList() ?? [];

            if (selected.Count == 0)
                throw new SecurityFailure(400, "Checkout.NoItemsSelected", "Selecione ao menos um item do carrinho para finalizar a compra.");

            var unavailable = selected.Where(item => item.GetAvailability(settings.AllowBackorder, visible) != CartItemAvailability.Available).ToList();

            if (unavailable.Count > 0)
                throw new SecurityFailure(409, "Checkout.ItemsUnavailable",
                    $"Itens indisponíveis: {string.Join(", ", unavailable.Select(Item =>
                        Item.Product.IsContractBlocked ? "Produto indisponível" : Item.Product.Name))}. Atualize o carrinho.");

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

            // A seleção consulta apenas configuração local. Pedido gratuito não depende de uma conexão financeira.
            var providerContext = order.TotalAmount == 0
                ? new PaymentProviderContext(IntegrationProviders.Manual, IntegrationEnvironment.Sandbox, $"manual-{enterpriseId}", null)
                : await gateways.GetContextAsync(enterpriseId, ct);

            ReserveCouponUse(order, now);
            ReserveStock(order, selected, settings, key, expiresAt, now);
            var payment = Transaction.CreatePendingPayment(order, request.PaymentMethodId, providerContext.Provider);
            transactions.Add(payment);

            if (order.TotalAmount > 0)
                paymentAttempts.Add(PaymentAttempt.Create(payment, providerContext.Provider, providerContext.Environment,
                    providerContext.ExternalAccountId, providerContext.ProviderConnectionId, settings.CurrencyCode,
                    customer.FullName, customer.Email.Value, expiresAt, now));

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

        if (string.IsNullOrWhiteSpace(customer.AddressLine) || string.IsNullOrWhiteSpace(customer.City) ||
            string.IsNullOrWhiteSpace(customer.ZipCode) || !customer.StateId.HasValue)
            throw new SecurityFailure(400, "Checkout.AddressRequired", "Cadastre um endereço de entrega para finalizar a compra.");

        return (customer.AddressLine, customer.City, customer.StateId.Value, customer.ZipCode);
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
        var tracked = selected.Where(item => OrderStockPolicy.ControlsStock(item.ProductVariant)).ToList();

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

    private void ReserveCouponUse(Order order, DateTime now)
    {
        if (order.Coupon is not { } coupon || order.DiscountAmount <= 0)
            return;

        // O bloqueio da loja serializa a verificação e o consumo da última utilização disponível.
        // A transação do checkout desfaz o compromisso se estoque, pedido ou pagamento local não puderem ser gravados.
        coupon.RegisterUse(now);
        redemptions.Add(CouponRedemption.Create(coupon, order, now));
    }

    /// <summary>
    /// Garante a cobrança do pedido: pedidos sem valor a pagar (ex.: cupom de 100%) são confirmados na hora;
    /// os demais processam a intenção persistida com exclusão mútua entre requisições e workers.
    /// </summary>
    private async Task<OrderDetailResponse> CompleteAsync(long enterpriseId, long customerId, long orderId, CancellationToken ct)
    {
        var order = await orders.GetForWorkflowAsync(enterpriseId, orderId, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        if (order.IsAwaitingPayment && order.TotalAmount == 0)
        {
            await ConfirmZeroPaymentAsync(enterpriseId, orderId, ct);
        }
        else if (order.IsAwaitingPayment)
        {
            var attemptId = await paymentAttempts.FindForOrderAsync(enterpriseId, orderId, ct);

            if (attemptId is null)
            {
                var settings = await storeSettings.GetForEnterpriseAsync(enterpriseId, ct);
                await paymentAttempts.EnsureLegacyAttemptAsync(enterpriseId, orderId, settings.CurrencyCode, ct);
                attemptId = await paymentAttempts.FindForOrderAsync(enterpriseId, orderId, ct);
            }

            if (attemptId is long id)
                await sender.Send(new ProcessPaymentAttemptCommand(enterpriseId, id), ct);
        }

        var detail = await orders.GetDetailAsync(orderId, customerId, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        return OrderDetailResponse.FromEntity(detail, backoffice: false);
    }

    private async Task<Order> ConfirmZeroPaymentAsync(long enterpriseId, long orderId, CancellationToken ct)
    {
        long? customerId = null;
        var source = OrderChangeSource.System;
        long? actorId = null;
        long? transactionMethodId = null;
        var gatewayReference = "sem-cobranca";
        decimal? paidAmount = 0;
        var note = "Pedido sem valor a pagar";

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);

        var order = await orders.GetForWorkflowAsync(enterpriseId, orderId, ct);

        if (order is null || (customerId.HasValue && order.CustomerId != customerId))
            throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        using (scope.Begin(enterpriseId, customerId))
        {
            await ApplyAsync();
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return order;

        async Task ApplyAsync()
        {
            if (order.IsPaid)
                return;

            if (!order.IsAwaitingPayment)
                throw new SecurityFailure(409, "Order.NotAwaitingPayment", "O pedido não está aguardando pagamento.");

            if (paidAmount.HasValue && paidAmount.Value != order.TotalAmount)
                throw new SecurityFailure(409, "Payment.AmountMismatch", "O valor pago é diferente do total do pedido.");

            var now = Now;
            var settings = await storeSettings.GetForEnterpriseAsync(enterpriseId, ct);
            var payment = order.ListTransaction.Where(transaction => transaction.IsPendingPayment).OrderByDescending(transaction => transaction.Id).FirstOrDefault();

            if (source == OrderChangeSource.Backoffice &&
                (order.ListTransaction.Any(Transaction => Transaction.GatewayProvider != IntegrationProviders.Manual) ||
                    order.ListPaymentAttempt.Any(Attempt => Attempt.Provider != IntegrationProviders.Manual)))
                throw new SecurityFailure(409, "Payment.ProviderConfirmationRequired",
                    "Pagamentos externos exigem confirmação autenticada do provedor.");

            if (payment is null)
            {
                var method = transactionMethodId ?? order.PaymentMethodId
                    ?? throw new SecurityFailure(400, "Payment.MethodRequired", "Informe a forma de pagamento recebida.");
                payment = Transaction.CreatePendingPayment(order, method, IntegrationProviders.Manual);
                transactions.Add(payment);
            }

            stock.CommitStock(order, settings, now);
            order.ConfirmPayment(settings, source, actorId, note, now);
            payment.MarkCaptured(gatewayReference, now);
            await couponUses.RegisterCouponUseAsync(order, now, ct);
        }
    }
}
