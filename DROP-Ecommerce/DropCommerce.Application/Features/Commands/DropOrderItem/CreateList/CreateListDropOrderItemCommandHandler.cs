using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropOrderItemCommandHandler(IRepository<DropOrderItem> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropOrderItemCommand, CreateListDropOrderItemCommand, DropOrderItem>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropOrderItemCommand> GetCommandList(CreateListDropOrderItemCommand request) => request.commands;

    protected override DropOrderItem CreateEntity(CreateDropOrderItemCommand command) =>
        DropOrderItem.Create(command.dropOrderId, command.dropProductId, command.itemName, command.sku, command.quantity, command.unitPrice, command.totalPrice);
}
