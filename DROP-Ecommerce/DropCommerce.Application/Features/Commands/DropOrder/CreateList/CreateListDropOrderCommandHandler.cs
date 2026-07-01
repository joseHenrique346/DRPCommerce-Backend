using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropOrderCommandHandler(IRepository<DropOrder> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropOrderCommand, CreateListDropOrderCommand, DropOrder>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropOrderCommand> GetCommandList(CreateListDropOrderCommand request) => request.commands;

    protected override DropOrder CreateEntity(CreateDropOrderCommand command) =>
        DropOrder.Create(command.dropEventId, command.customerId, command.reservationId, command.couponId, command.statusId, command.paymentStatusId, command.subTotal, command.discountAmount, command.shippingCost, command.taxAmount, command.totalAmount, command.shippingAddressLine, command.shippingCity, command.shippingState, command.shippingZipCode, command.notes);
}
