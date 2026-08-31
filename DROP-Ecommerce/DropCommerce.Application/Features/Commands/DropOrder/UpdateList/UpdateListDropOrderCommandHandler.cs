using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropOrderCommandHandler(IRepository<DropOrder> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropOrderCommand, UpdateListDropOrderCommand, DropOrder>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropOrderCommand> GetCommandList(UpdateListDropOrderCommand request) => request.commands;

    protected override long GetById(UpdateDropOrderCommand command) => command.id;

    protected override void ApplyChanges(DropOrder entity, UpdateDropOrderCommand command)
    {
        entity.Update(command.dropEventId, command.customerId, command.reservationId, command.couponId, command.statusId, command.paymentStatusId, command.subTotal, command.discountAmount, command.shippingCost, command.taxAmount, command.totalAmount, command.shippingAddressLine, command.shippingCity, command.shippingState, command.shippingZipCode, command.notes);
    }
}
