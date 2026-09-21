using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListCouponCommandHandler(IRepository<Coupon> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateCouponCommand, UpdateListCouponCommand, Coupon>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateCouponCommand> GetCommandList(UpdateListCouponCommand request) => request.commands;

    protected override long GetById(UpdateCouponCommand command) => command.id;

    protected override void ApplyChanges(Coupon entity, UpdateCouponCommand command)
    {
        entity.UpdateDetails(command.code, command.typeId, command.discountValue, command.minOrderValue, command.maxDiscountCap, command.maxUses, command.isSingleUse, command.startsAt, command.expiresAt);
    }
}
