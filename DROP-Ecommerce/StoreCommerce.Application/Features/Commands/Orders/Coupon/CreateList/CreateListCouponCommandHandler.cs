using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;
using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCouponCommandHandler(ICouponRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : IRequestHandler<CreateListCouponCommand, Result<List<Coupon>>>
{
    public async Task<Result<List<Coupon>>> Handle(CreateListCouponCommand request, CancellationToken cancellationToken)
    {
        var error = await CouponRules.ValidateUniqueCodesAsync(repository, request.commands.Select(command => (0L, command.code)).ToList(), cancellationToken);

        if (error is not null)
            return Result<List<Coupon>>.Failure(error);

        var enterpriseId = tenantProvider.GetEnterpriseId();
        var coupons = request.commands.Select(command => Coupon.Create(enterpriseId, command.code, command.typeId, command.discountValue,
            command.maxDiscountCap, command.maxUses, command.isActive, command.isSingleUse, command.startsAt, command.expiresAt,
            CouponRules.BuildConditions(command.conditions))).ToList();

        await repository.AddRangeAsync(coupons, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<List<Coupon>>.Success(coupons);
    }
}
