using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class OrderCouponRedemptionWriter(
    IRepository<CouponRedemption> redemptions)
{
    public async Task RegisterCouponUseAsync(Order order, DateTime now, CancellationToken ct)
    {
        if (order.Coupon is not { } coupon || order.DiscountAmount <= 0)
            return;

        if (await redemptions.ExistsAsync(redemption => redemption.EnterpriseId == order.EnterpriseId && redemption.OrderId == order.Id &&
                redemption.ReleasedAt == null, ct, ignoreQueryFilters: true))
            return;

        // Pedidos criados diretamente no backoffice não comprometem cupom enquanto são rascunhos.
        // O checkout já possui resgate ativo e não chega a este incremento.
        coupon.RegisterConfirmedUse();
        redemptions.Add(CouponRedemption.Create(coupon, order, now));
    }

    public async Task ReleaseCouponUseAsync(Order order, DateTime now, CancellationToken ct)
    {
        var active = await redemptions.ListAsync(redemption => redemption.EnterpriseId == order.EnterpriseId && redemption.OrderId == order.Id &&
            redemption.ReleasedAt == null, ct, ignoreQueryFilters: true);

        foreach (var redemption in active)
        {
            redemption.Release(now);
            order.Coupon?.ReleaseUse();
        }
    }
}
