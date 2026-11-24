using StoreCommerce.Domain.Interfaces.Persistence;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

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
        long customerId, long? orderId, CouponCustomerHistoryReader eligibility, DateTime now, CancellationToken cancellationToken)
    {
        if (!coupons.TryGetValue(couponId, out var coupon))
            return (Error.NotFound("Coupon.NotFound", $"Coupon com id {couponId} não encontrado."), null);

        var history = await eligibility.GetHistoryAsync(couponId, customerId, orderId, cancellationToken);

        return (ValidateForApply(coupon, history, now), history);
    }

    /// <summary>
    /// Valida o cupom no momento em que ele é vinculado ao pedido: disponibilidade e condições do cliente.
    /// </summary>
    public static Error? ValidateForApply(Coupon coupon, CouponCustomerHistory history, DateTime now)
    {
        var failure = coupon.CheckAvailability(now, history) ?? coupon.CheckCustomerConditions(history);

        return failure is null ? null : Error.Validation(failure.ErrorCode!, failure.Message!);
    }
}
