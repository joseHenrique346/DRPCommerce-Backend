using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListOrderItemCommandHandler(IRepository<OrderItem> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateOrderItemCommand, CreateListOrderItemCommand, OrderItem>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateOrderItemCommand> GetCommandList(CreateListOrderItemCommand request) => request.commands;

    protected override OrderItem CreateEntity(CreateOrderItemCommand command) =>
        OrderItem.Create(command.orderId, command.productId, command.serviceId, command.itemName, command.sku, command.quantity, command.unitPrice, command.discountAmount, command.totalPrice);
}
