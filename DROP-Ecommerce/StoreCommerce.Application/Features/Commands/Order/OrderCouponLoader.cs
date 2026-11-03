using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Carrega os cupons informados nos comandos de pedido (com as condições) e valida a vinculação ao pedido.
/// </summary>
internal static class OrderCouponLoader
{
    public static async Task<Dictionary<long, Coupon>> LoadAsync(IRepository<Coupon> repository, IEnumerable<long?> couponIds, CancellationToken cancellationToken)
    {
        var ids = couponIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

        if (ids.Count == 0)
            return [];

        return (await repository.GetListByListIdAsync(ids, cancellationToken)).ToDictionary(coupon => coupon.Id);
    }

    /// <summary>
    /// Valida a vinculação do cupom ao pedido do cliente e devolve o histórico usado, para aplicar o cupom sem nova consulta.
    /// </summary>
    public static async Task<(Error? Error, CouponCustomerHistory? History)> ValidateAsync(IReadOnlyDictionary<long, Coupon> coupons, long couponId,
        long customerId, long? orderId, CouponEligibilityService eligibility, DateTime now, CancellationToken cancellationToken)
    {
        if (!coupons.TryGetValue(couponId, out var coupon))
            return (Error.NotFound("Coupon.NotFound", $"Coupon com id {couponId} não encontrado."), null);

        var history = await eligibility.GetHistoryAsync(couponId, customerId, orderId, cancellationToken);

        return (CouponEligibilityService.ValidateForApply(coupon, history, now), history);
    }
}
