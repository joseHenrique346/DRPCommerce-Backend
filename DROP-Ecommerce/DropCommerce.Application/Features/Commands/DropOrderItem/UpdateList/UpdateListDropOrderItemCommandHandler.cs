using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropOrderItemCommandHandler(IRepository<DropOrderItem> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropOrderItemCommand, UpdateListDropOrderItemCommand, DropOrderItem>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropOrderItemCommand> GetCommandList(UpdateListDropOrderItemCommand request) => request.commands;

    protected override long GetById(UpdateDropOrderItemCommand command) => command.id;

    protected override void ApplyChanges(DropOrderItem entity, UpdateDropOrderItemCommand command)
    {
        entity.Update(command.dropOrderId, command.dropProductId, command.itemName, command.sku, command.quantity, command.unitPrice, command.totalPrice);
    }
}
