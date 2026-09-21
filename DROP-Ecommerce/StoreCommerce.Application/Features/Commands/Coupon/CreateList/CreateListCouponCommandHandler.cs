using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCouponCommandHandler(IRepository<Coupon> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateCouponCommand, CreateListCouponCommand, Coupon>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateCouponCommand> GetCommandList(CreateListCouponCommand request) => request.commands;

    protected override Coupon CreateEntity(CreateCouponCommand command) =>
        Coupon.Create(command.enterpriseId, command.code, command.typeId, command.discountValue, command.minOrderValue, command.maxDiscountCap, command.maxUses, command.usedCount, command.isActive, command.isSingleUse, command.startsAt, command.expiresAt);
}
