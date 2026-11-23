using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CouponCustomerHistoryReader(
    IOrderRepository orderRepository,
    IRepository<CouponRedemption> redemptionRepository)
{
    public async Task<CouponCustomerHistory> GetHistoryAsync(long couponId, long customerId, long? excludeOrderId, CancellationToken cancellationToken)
    {
        var (completedOrders, totalSpent) = await orderRepository.GetCustomerPurchaseSummaryAsync(customerId, excludeOrderId, cancellationToken);

        var pending = OrderStatus.Pending.Id;

        // Para cupons de uso único, um pedido ainda aguardando pagamento com o mesmo cupom também conta como uso,

        // evitando que o cliente abra dois checkouts e confirme os dois.

        var redeemed =

            await redemptionRepository.ExistsAsync(redemption => redemption.CouponId == couponId && redemption.CustomerId == customerId &&

                redemption.ReleasedAt == null && (excludeOrderId == null || redemption.OrderId != excludeOrderId), cancellationToken) ||

            await orderRepository.ExistsAsync(order => order.CustomerId == customerId && order.CouponId == couponId &&

                order.OrderStatusId == pending && order.IsLocked && (excludeOrderId == null || order.Id != excludeOrderId), cancellationToken);

        return new CouponCustomerHistory(completedOrders, totalSpent, redeemed);
    }
}
