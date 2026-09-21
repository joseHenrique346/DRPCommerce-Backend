using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListOrderCommandHandler(IRepository<Order> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateOrderCommand, UpdateListOrderCommand, Order>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateOrderCommand> GetCommandList(UpdateListOrderCommand request) => request.commands;

    protected override long GetById(UpdateOrderCommand command) => command.id;

    protected override void ApplyChanges(Order entity, UpdateOrderCommand command)
    {
        entity.UpdateStatus(command.statusId, command.paymentStatusId);
        entity.UpdateShippingAddress(command.shippingAddressLine, command.shippingCity, 0, command.shippingZipCode, command.notes);
        entity.UpdateAmounts(command.subTotal, command.discountAmount, command.shippingCost, command.taxAmount, command.totalAmount);
    }
}
