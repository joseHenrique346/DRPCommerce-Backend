using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCouponCommandHandler(IRepository<Coupon> repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateCouponCommand, CreateListCouponCommand, Coupon>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateCouponCommand> GetCommandList(CreateListCouponCommand request) => request.commands;

    protected override Coupon CreateEntity(CreateCouponCommand command) =>
        Coupon.Create(tenantProvider.GetEnterpriseId(), command.code, command.typeId, command.discountValue, command.minOrderValue, command.maxDiscountCap, command.maxUses, command.isActive, command.isSingleUse, command.startsAt, command.expiresAt);
}
