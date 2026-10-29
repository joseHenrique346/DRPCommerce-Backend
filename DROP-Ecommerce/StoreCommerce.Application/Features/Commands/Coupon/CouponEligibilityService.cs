using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Monta o histórico do cliente para as condições do cupom e controla o resgate (uso efetivo) conforme o status do pedido.
/// </summary>
public sealed class CouponEligibilityService(
    IOrderRepository orderRepository,
    IRepository<CouponRedemption> redemptionRepository,
    IRepository<Coupon> couponRepository)
{
    // Resgates feitos nesta requisição e ainda não salvos (vários pedidos no mesmo lote).
    private readonly HashSet<(long CouponId, long CustomerId)> _pendingRedemptions = [];

    public async Task<CouponCustomerHistory> GetHistoryAsync(long couponId, long customerId, long? excludeOrderId, CancellationToken cancellationToken)
    {
        var (completedOrders, totalSpent) = await orderRepository.GetCustomerPurchaseSummaryAsync(customerId, excludeOrderId, cancellationToken);
        var redeemed = _pendingRedemptions.Contains((couponId, customerId)) ||
            await redemptionRepository.ExistsAsync(redemption => redemption.CouponId == couponId && redemption.CustomerId == customerId &&
                redemption.ReleasedAt == null && (excludeOrderId == null || redemption.OrderId != excludeOrderId), cancellationToken);

        return new CouponCustomerHistory(completedOrders, totalSpent, redeemed);
    }

    /// <summary>
    /// Valida o cupom no momento em que ele é vinculado ao pedido: disponibilidade e condições do cliente.
    /// </summary>
    public static Error? ValidateForApply(Coupon coupon, CouponCustomerHistory history, DateTime now)
    {
        var failure = coupon.CheckAvailability(now, history) ?? coupon.CheckCustomerConditions(history);

        return failure is null ? null : Error.Validation(failure.ErrorCode!, failure.Message!);
    }

    /// <summary>
    /// Sincroniza o resgate com o status atual do pedido: registra o uso quando o pedido passa a contar como compra
    /// (todas as regras do cupom são exigidas) e libera quando ele é cancelado, reembolsado ou volta a Pendente.
    /// </summary>
    public async Task<Error?> SyncRedemptionAsync(Order order, DateTime now, CancellationToken cancellationToken)
    {
        var active = await redemptionRepository.FindAsync(redemption => redemption.OrderId == order.Id && redemption.ReleasedAt == null, cancellationToken);

        if (!Order.ConsumesCoupon(order.OrderStatusId))
        {
            if (active is not null)
                Release(active, order.Coupon, now);

            return null;
        }

        if (active is not null || order.Coupon is null)
            return null;

        var coupon = order.Coupon;
        var history = await GetHistoryAsync(coupon.Id, order.CustomerId, order.Id, cancellationToken);
        var evaluation = coupon.Evaluate(now, order.SubTotal, order.TotalQuantity, history);

        if (!evaluation.IsValid)
            return Error.Validation(evaluation.ErrorCode!,
                $"Pedido {order.Id}: {evaluation.Message} Remova o cupom ou ajuste o pedido antes de sair de Pendente.");

        coupon.RegisterUse(now);
        redemptionRepository.Add(CouponRedemption.Create(coupon, order, now));
        _pendingRedemptions.Add((coupon.Id, order.CustomerId));

        return null;
    }

    /// <summary>
    /// Libera os resgates ativos dos pedidos informados (usado na exclusão de pedidos).
    /// </summary>
    public async Task ReleaseForOrdersAsync(IReadOnlyCollection<long> orderIds, DateTime now, CancellationToken cancellationToken)
    {
        var active = await redemptionRepository.ListAsync(redemption => orderIds.Contains(redemption.OrderId) && redemption.ReleasedAt == null, cancellationToken);

        if (active.Count == 0)
            return;

        var couponIds = active.Select(redemption => redemption.CouponId).Distinct().ToList();
        var enterpriseIds = active.Select(redemption => redemption.EnterpriseId).Distinct().ToList();

        // Cupons excluídos logicamente também precisam devolver o uso; o filtro de loja é refeito manualmente.
        var coupons = (await couponRepository.ListAsync(coupon => couponIds.Contains(coupon.Id) && enterpriseIds.Contains(coupon.EnterpriseId),
                cancellationToken, ignoreQueryFilters: true))
            .ToDictionary(coupon => coupon.Id);

        foreach (var redemption in active)
            Release(redemption, coupons.GetValueOrDefault(redemption.CouponId), now);
    }

    private static void Release(CouponRedemption redemption, Coupon? coupon, DateTime now)
    {
        redemption.Release(now);
        coupon?.ReleaseUse();
    }
}
