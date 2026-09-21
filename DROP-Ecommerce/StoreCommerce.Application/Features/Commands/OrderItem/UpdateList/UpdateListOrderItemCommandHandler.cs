using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListOrderItemCommandHandler(IRepository<OrderItem> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateOrderItemCommand, UpdateListOrderItemCommand, OrderItem>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateOrderItemCommand> GetCommandList(UpdateListOrderItemCommand request) => request.commands;

    protected override long GetById(UpdateOrderItemCommand command) => command.id;

    protected override void ApplyChanges(OrderItem entity, UpdateOrderItemCommand command)
    {
        entity.UpdateDetails(command.itemName, command.sku, command.quantity, command.unitPrice, command.discountAmount, command.totalPrice);
    }
}
