using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropCouponCommandHandler(IRepository<DropCoupon> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropCouponCommand, UpdateListDropCouponCommand, DropCoupon>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropCouponCommand> GetCommandList(UpdateListDropCouponCommand request) => request.commands;

    protected override long GetById(UpdateDropCouponCommand command) => command.id;

    protected override void ApplyChanges(DropCoupon entity, UpdateDropCouponCommand command)
    {
        entity.Update(command.dropEventId, command.code, command.typeId, command.discountValue, command.minOrderValue, command.maxDiscountCap, command.maxUses, command.usedCount, command.isActive, command.isSingleUse, command.isExclusiveToRegistered, command.startsAt, command.expiresAt);
    }
}
