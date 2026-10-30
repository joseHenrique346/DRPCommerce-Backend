using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Orders;

/// <summary>
/// Etapas sem dados extras. O envio tem fluxo próprio (ShipAsync), porque registra transportadora e rastreio.
/// </summary>
public enum OrderStep
{
    StartProcessing = 1,
    Deliver = 3
}

/// <summary>
/// Dados do envio. Sem transportadora na retirada no local (ShipmentType.Pickup).
/// </summary>
public sealed record ShipmentDetails(long ShipmentTypeId, string? CarrierName, string? TrackingCode, DateTime EstimatedDelivery);

/// <summary>
/// Máquina de estados do pedido com seus efeitos: baixa e devolução de estoque, uso do cupom e pagamento.
/// Trabalha sempre com a loja informada (sem depender da requisição), para servir backoffice, vitrine, worker e webhook.
/// </summary>
public sealed class OrderLifecycleService(
    IOrderRepository orders,
    IRepository<Transaction> transactions,
    IRepository<CouponRedemption> redemptions,
    IInventoryRepository inventory,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    StoreSettingsReader storeSettings,
    IPaymentGateway gateway,
    OrderWorkflowScope scope,
    TimeProvider time)
{
    public const string ExpiredReason = "Pagamento não confirmado no prazo.";

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Pagamento aprovado: confirma o pedido, baixa o estoque (da reserva ou, se ela expirou, do saldo atual) e conta o uso do cupom.
    /// Repetir a confirmação de um pedido já pago não faz nada.
    /// </summary>
    public Task<Order> ConfirmPaymentAsync(long enterpriseId, long orderId, OrderChangeSource source, long? actorId,
        long? transactionMethodId, string? gatewayReference, decimal? paidAmount, string? note, CancellationToken ct) =>
        RunAsync(enterpriseId, orderId, null, async order =>
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

            if (payment is null)
            {
                var method = transactionMethodId ?? order.PaymentMethodId
                    ?? throw new SecurityFailure(400, "Payment.MethodRequired", "Informe a forma de pagamento recebida.");
                payment = Transaction.CreatePendingPayment(order, method, gateway.Provider);
                transactions.Add(payment);
            }

            CommitStock(order, settings, now);
            order.ConfirmPayment(settings, source, actorId, note, now);
            payment.MarkCaptured(gatewayReference, now);
            await RegisterCouponUseAsync(order, now, ct);
        }, ct);

    public Task<Order> AdvanceAsync(long enterpriseId, long orderId, OrderStep step, OrderChangeSource source, long? actorId, string? note,
        CancellationToken ct) =>
        RunAsync(enterpriseId, orderId, null, order =>
        {
            var now = Now;

            switch (step)
            {
                case OrderStep.StartProcessing:
                    order.StartProcessing(source, actorId, now);
                    break;
                default:
                    order.MarkDelivered(source, actorId, now);
                    break;
            }

            return Task.CompletedTask;
        }, ct);

    /// <summary>
    /// Em processamento → Enviado. Pedidos com produtos físicos exigem os dados do envio; pedidos só com itens digitais ou
    /// serviços podem ser marcados como enviados sem eles.
    /// </summary>
    public Task<Order> ShipAsync(long enterpriseId, long orderId, ShipmentDetails? details, OrderChangeSource source, long? actorId, string? note,
        CancellationToken ct) =>
        RunAsync(enterpriseId, orderId, null, order =>
        {
            var now = Now;
            var physical = order.ListOrderItem.Any(item => item.ProductId.HasValue && item.ProductVariant?.Product.IsDigital != true);

            if (physical && details is null)
                throw new SecurityFailure(400, "Order.ShipmentRequired", "Informe o tipo de envio, a transportadora e a previsão de entrega.");

            var shipment = details is null ? null : Shipment.CreateForOrder(order, details.ShipmentTypeId, details.CarrierName, details.TrackingCode,
                details.EstimatedDelivery, now);

            order.MarkShipped(shipment, source, actorId, note, now);

            return Task.CompletedTask;
        }, ct);

    /// <summary>
    /// Cancela o pedido: devolve o estoque (libera a reserva ou repõe o que já foi baixado), devolve o uso do cupom e,
    /// se o pedido estava pago, estorna o pagamento pelo gateway. Com customerId, só o próprio cliente cancela e apenas
    /// pedidos que ainda aguardam pagamento.
    /// </summary>
    public async Task<Order> CancelAsync(long enterpriseId, long orderId, string reason, OrderChangeSource source, long? actorId,
        long? customerId, CancellationToken ct)
    {
        string? gatewayReferenceToCancel = null;

        var cancelled = await RunAsync(enterpriseId, orderId, customerId, async order =>
        {
            if (order.IsCancelled)
                return;

            if (customerId.HasValue && !order.IsAwaitingPayment)
                throw new SecurityFailure(409, "Order.CancelNotAllowed",
                    "Pedidos já pagos só podem ser cancelados pela loja. Entre em contato com o atendimento.");

            if (!order.CanBeCancelled)
                throw new SecurityFailure(409, "Order.CancelNotAllowed", "Pedidos enviados ou entregues não podem ser cancelados.");

            var now = Now;
            var wasPaid = order.IsPaid;
            ReturnStock(order, now);
            await ReleaseCouponUseAsync(order, now, ct);
            order.Cancel(reason, source, actorId, now);

            if (wasPaid)
            {
                await RefundAsync(order, reason, source, actorId, now, ct);
            }
            else
            {
                var pending = order.ListTransaction.FirstOrDefault(transaction => transaction.IsPendingPayment);
                gatewayReferenceToCancel = pending?.GatewayReference;
                pending?.MarkCancelled();
            }
        }, ct);

        await CancelGatewayPaymentAsync(gatewayReferenceToCancel, ct);

        return cancelled;
    }

    /// <summary>
    /// Pagamento recusado pelo gateway: cancela o pedido pendente e libera a reserva.
    /// </summary>
    public Task<Order> FailPaymentAsync(long enterpriseId, long orderId, string reason, OrderChangeSource source, CancellationToken ct) =>
        RunAsync(enterpriseId, orderId, null, order =>
        {
            if (!order.IsAwaitingPayment)
                return Task.CompletedTask;

            var now = Now;
            ReturnStock(order, now);
            order.MarkPaymentFailed(reason, source, null, now);

            foreach (var payment in order.ListTransaction.Where(transaction => transaction.IsPendingPayment))
                payment.MarkFailed();

            return Task.CompletedTask;
        }, ct);

    /// <summary>
    /// Resultado de pagamento vindo do gateway (webhook). Aprovação de pedido já cancelado (ex.: Pix pago após expirar)
    /// é estornada automaticamente; eventos repetidos não têm efeito.
    /// </summary>
    public async Task<Order> ApplyPaymentResultAsync(string provider, string reference, PaymentResultStatus status, decimal? paidAmount,
        CancellationToken ct)
    {
        var owner = await orders.FindByPaymentReferenceAsync(provider, reference, ct)
            ?? throw new SecurityFailure(404, "Payment.NotFound", "Cobrança não encontrada.");

        if (status == PaymentResultStatus.Failed)
            return await FailPaymentAsync(owner.EnterpriseId, owner.OrderId, "Pagamento recusado pelo gateway.", OrderChangeSource.PaymentGateway, ct);

        var current = await orders.GetForWorkflowAsync(owner.EnterpriseId, owner.OrderId, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        if (!current.IsCancelled)
            return await ConfirmPaymentAsync(owner.EnterpriseId, owner.OrderId, OrderChangeSource.PaymentGateway, null, null, reference,
                paidAmount, "Pagamento aprovado pelo gateway", ct);

        return await RunAsync(owner.EnterpriseId, owner.OrderId, null, async order =>
        {
            if (order.ListTransaction.Any(transaction => transaction.TransactionTypeId == Domain.StaticEntity.TransactionType.Refund.Id))
                return;

            var now = Now;
            var payment = order.ListTransaction.First(transaction => transaction.GatewayReference == reference);
            payment.MarkCaptured(reference, now);
            await RefundAsync(order, "Pagamento aprovado após o cancelamento do pedido.", OrderChangeSource.PaymentGateway, null, now, ct);
        }, ct);
    }

    /// <summary>
    /// Cancela os pedidos pendentes da loja cujo prazo de pagamento venceu. Chamado pelo worker de expiração.
    /// </summary>
    public async Task<int> ExpirePendingAsync(long enterpriseId, CancellationToken ct)
    {
        var expired = await orders.GetExpiredPaymentOrderIdsAsync(enterpriseId, Now, 50, ct);

        foreach (var orderId in expired)
            await CancelAsync(enterpriseId, orderId, ExpiredReason, OrderChangeSource.System, null, null, ct);

        return expired.Count;
    }

    private async Task<Order> RunAsync(long enterpriseId, long orderId, long? customerId, Func<Order, Task> change, CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        // Mesmo bloqueio do estoque e do catálogo: confirmações, cancelamentos e reservas da loja não se cruzam.
        await products.LockCatalogAsync(enterpriseId, ct);

        var order = await orders.GetForWorkflowAsync(enterpriseId, orderId, ct);

        if (order is null || (customerId.HasValue && order.CustomerId != customerId))
            throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        using (scope.Begin(enterpriseId, customerId))
        {
            await change(order);
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return order;
    }

    private void CommitStock(Order order, StoreSettings settings, DateTime now)
    {
        var reservation = order.StockReservation;

        if (reservation is null || reservation.Status != StockReservationStatus.Reserved)
        {
            // Pedido do backoffice (sem reserva) ou reserva expirada: reserva agora com o saldo atual e baixa em seguida.
            var lines = order.ListOrderItem.Where(item => item.ProductVariant is { } variant && ControlsStock(variant)).ToList();

            if (lines.Count == 0)
                return;

            reservation = StockReservation.Create(order.EnterpriseId, $"order-{order.Id}-confirm", $"Pedido {order.Id}", now);

            foreach (var item in lines)
            {
                var variant = item.ProductVariant!;

                if (!settings.AllowBackorder && variant.AvailableQuantity < item.Quantity)
                    throw new SecurityFailure(409, "Order.StockUnavailable", $"Estoque insuficiente para o SKU {variant.SKU}.");

                reservation.AddLine(variant, item.Quantity, settings.AllowBackorder);
                variant.ReserveStock(item.Quantity, settings.AllowBackorder);
                inventory.AddMovement(StockMovement.Create(variant, reservation, StockMovementType.Reservation, reservation.IdempotencyKey,
                    0, item.Quantity, "Reserva na confirmação do pedido", null, now));
            }

            inventory.AddReservation(reservation);
            order.AttachStockReservation(reservation);
        }

        foreach (var line in reservation.Lines)
        {
            line.ProductVariant.CommitStock(line.Quantity);
            inventory.AddMovement(StockMovement.Create(line.ProductVariant, reservation, StockMovementType.Commit, reservation.IdempotencyKey,
                -line.Quantity, -line.Quantity, $"Baixa do pedido {order.Id}", null, now));
        }

        reservation.Complete(StockReservationStatus.Committed, now);
    }

    private void ReturnStock(Order order, DateTime now)
    {
        var reservation = order.StockReservation;

        if (reservation is null)
            return;

        if (reservation.Status == StockReservationStatus.Reserved)
        {
            foreach (var line in reservation.Lines)
            {
                line.ProductVariant.ReleaseStock(line.Quantity);
                inventory.AddMovement(StockMovement.Create(line.ProductVariant, reservation, StockMovementType.Release, reservation.IdempotencyKey,
                    0, -line.Quantity, $"Cancelamento do pedido {order.Id}", null, now));
            }

            reservation.Complete(StockReservationStatus.Released, now);
        }
        else if (reservation.Status == StockReservationStatus.Committed)
        {
            foreach (var line in reservation.Lines)
            {
                line.ProductVariant.AdjustStock(line.Quantity, allowBackorder: true);
                inventory.AddMovement(StockMovement.Create(line.ProductVariant, reservation, StockMovementType.Return, $"order-{order.Id}-return",
                    line.Quantity, 0, $"Devolução do cancelamento do pedido {order.Id}", null, now));
            }
        }
    }

    private async Task RegisterCouponUseAsync(Order order, DateTime now, CancellationToken ct)
    {
        if (order.Coupon is not { } coupon || order.DiscountAmount <= 0)
            return;

        if (await redemptions.ExistsAsync(redemption => redemption.EnterpriseId == order.EnterpriseId && redemption.OrderId == order.Id &&
                redemption.ReleasedAt == null, ct, ignoreQueryFilters: true))
            return;

        coupon.RegisterConfirmedUse();
        redemptions.Add(CouponRedemption.Create(coupon, order, now));
    }

    private async Task ReleaseCouponUseAsync(Order order, DateTime now, CancellationToken ct)
    {
        var active = await redemptions.ListAsync(redemption => redemption.EnterpriseId == order.EnterpriseId && redemption.OrderId == order.Id &&
            redemption.ReleasedAt == null, ct, ignoreQueryFilters: true);

        foreach (var redemption in active)
        {
            redemption.Release(now);
            order.Coupon?.ReleaseUse();
        }
    }

    private async Task RefundAsync(Order order, string reason, OrderChangeSource source, long? actorId, DateTime now, CancellationToken ct)
    {
        var payment = order.ListTransaction.FirstOrDefault(transaction => transaction.IsCapturedPayment);

        if (payment is null)
        {
            // Pedido marcado como pago antes do fluxo de pagamento existir: não há cobrança para estornar no gateway.
            order.RegisterFullRefund(source, actorId, "Estorno a ser feito fora do sistema (pagamento sem cobrança registrada)", now);

            return;
        }

        var result = await gateway.RefundAsync(new RefundRequest(order.EnterpriseId, order.Id, payment.GatewayReference, payment.Amount, reason,
            $"order-{order.Id}-refund"), ct);

        if (!result.Succeeded)
            throw new SecurityFailure(409, "Payment.RefundFailed", result.FailureReason ?? "O gateway recusou o estorno do pagamento.");

        transactions.Add(Transaction.CreateRefund(payment, result.Reference, now));
        order.RegisterFullRefund(source, actorId, "Pagamento estornado", now);
    }

    // Cancelar a cobrança no gateway é melhor esforço: se falhar, uma aprovação tardia é estornada por ApplyPaymentResultAsync.
    private async Task CancelGatewayPaymentAsync(string? reference, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return;

        try
        {
            await gateway.CancelPaymentAsync(reference, ct);
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            // O adaptador do gateway é responsável por registrar o erro em log.
        }
    }

    /// <summary>
    /// Produtos digitais e o SKU padrão de produtos com variantes não controlam estoque.
    /// </summary>
    public static bool ControlsStock(ProductVariant variant) =>
        !variant.Product.IsDigital && !(variant.Product.HasVariants && variant.IsDefault);
}
