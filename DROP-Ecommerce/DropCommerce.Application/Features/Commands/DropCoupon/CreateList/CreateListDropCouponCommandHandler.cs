using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropCouponCommandHandler(IRepository<DropCoupon> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropCouponCommand, CreateListDropCouponCommand, DropCoupon>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropCouponCommand> GetCommandList(CreateListDropCouponCommand request) => request.commands;

    protected override DropCoupon CreateEntity(CreateDropCouponCommand command) =>
        DropCoupon.Create(command.dropEventId, command.code, command.typeId, command.discountValue, command.minOrderValue, command.maxDiscountCap, command.maxUses, command.usedCount, command.isActive, command.isSingleUse, command.isExclusiveToRegistered, command.startsAt, command.expiresAt);
}
