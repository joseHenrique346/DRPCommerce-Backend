using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Altera a quantidade dos itens e recalcula os valores dos pedidos afetados.
/// </summary>
public class UpdateListOrderItemCommandHandler(
    IRepository<OrderItem> repository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateListOrderItemCommand, Result<List<OrderItem>>>
{
    public async Task<Result<List<OrderItem>>> Handle(UpdateListOrderItemCommand request, CancellationToken cancellationToken)
    {
        var listId = request.commands.Select(command => command.id).ToList();

        if (listId.Count != listId.Distinct().Count())
            return Result<List<OrderItem>>.Validation($"{nameof(UpdateListOrderItemCommand)}.DuplicateIds", "A lista contém ids duplicados.");

        var orderIdByItemId = await OrderItemLookup.GetOrderIdsAsync(repository, listId, cancellationToken);

        var missingId = listId.FirstOrDefault(id => !orderIdByItemId.ContainsKey(id));
        if (missingId > 0)
            return Result<List<OrderItem>>.NotFound("OrderItem.NotFound", $"OrderItem com id {missingId} não encontrado.");

        var orders = (await orderRepository.GetListWithItemsAsync(orderIdByItemId.Values.Distinct().ToList(), cancellationToken))
            .ToDictionary(order => order.Id);
        var items = new List<OrderItem>(request.commands.Count);

        foreach (var command in request.commands)
        {
            var order = orders[orderIdByItemId[command.id]];

            if (!order.IsEditable)
                return Result<List<OrderItem>>.Validation("Order.NotEditable", $"Os itens do pedido {order.Id} só podem ser alterados enquanto ele estiver pendente.");

            items.Add(order.UpdateItemQuantity(command.id, command.quantity));
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<List<OrderItem>>.Success(items);
    }
}
