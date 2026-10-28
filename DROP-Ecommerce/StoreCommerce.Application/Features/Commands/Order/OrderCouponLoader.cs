using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Carrega e valida os cupons informados nos comandos de pedido.
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

    public static Error? Validate(IReadOnlyDictionary<long, Coupon> coupons, long couponId, DateTime now)
    {
        if (!coupons.TryGetValue(couponId, out var coupon))
            return Error.NotFound("Coupon.NotFound", $"Coupon com id {couponId} não encontrado.");

        if (!coupon.IsUsable(now))
            return Error.Validation("Coupon.Unavailable", "Cupom inativo, fora da vigência ou sem usos disponíveis.");

        return null;
    }
}
