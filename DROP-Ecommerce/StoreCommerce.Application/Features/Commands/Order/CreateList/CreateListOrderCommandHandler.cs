using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListOrderCommandHandler(
    IRepository<Order> repository,
    IRepository<Coupon> couponRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider,
    StoreSettingsReader storeSettings,
    CouponEligibilityService eligibility,
    TimeProvider time)
    : IRequestHandler<CreateListOrderCommand, Result<List<Order>>>
{
    public async Task<Result<List<Order>>> Handle(CreateListOrderCommand request, CancellationToken cancellationToken)
    {
        if (!await storeSettings.IsStoreActiveAsync(cancellationToken))
            return Result<List<Order>>.Failure(StoreSettingsReader.InactiveStoreError);

        var enterpriseId = tenantProvider.GetEnterpriseId();
        var settings = await storeSettings.GetCurrentAsync(cancellationToken);
        var now = time.GetUtcNow().UtcDateTime;
        var coupons = await OrderCouponLoader.LoadAsync(couponRepository, request.commands.Select(command => command.couponId), cancellationToken);
        var orders = new List<Order>(request.commands.Count);

        foreach (var command in request.commands)
        {
            var order = Order.Create(enterpriseId, command.customerId, command.shippingAddressLine, command.shippingCity, command.shippingStateId, command.shippingZipCode, command.notes);

            if (command.couponId is long couponId)
            {
                var (error, history) = await OrderCouponLoader.ValidateAsync(coupons, couponId, command.customerId, null, eligibility, now, cancellationToken);

                if (error is not null)
                    return Result<List<Order>>.Failure(error);

                order.ApplyCoupon(coupons[couponId], history, now, settings);
            }

            orders.Add(order);
        }

        await repository.AddRangeAsync(orders, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<List<Order>>.Success(orders);
    }
}
