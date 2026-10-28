using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Remove itens dos pedidos e recalcula os valores dos pedidos afetados.
/// </summary>
public class DeleteListOrderItemCommandHandler(
    IRepository<OrderItem> repository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    StoreSettingsReader storeSettings)
    : IRequestHandler<DeleteListOrderItemCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteListOrderItemCommand request, CancellationToken cancellationToken)
    {
        if (!await storeSettings.IsStoreActiveAsync(cancellationToken))
            return Result<bool>.Failure(StoreSettingsReader.InactiveStoreError);

        var settings = await storeSettings.GetCurrentAsync(cancellationToken);
        var listId = request.ids.Distinct().ToList();
        var orderIdByItemId = await OrderItemLookup.GetOrderIdsAsync(repository, listId, cancellationToken);

        var missingId = listId.FirstOrDefault(id => !orderIdByItemId.ContainsKey(id));
        if (missingId > 0)
            return Result<bool>.NotFound("OrderItem.NotFound", $"OrderItem com id {missingId} não encontrado.");

        var orders = (await orderRepository.GetListWithItemsAsync(orderIdByItemId.Values.Distinct().ToList(), cancellationToken))
            .ToDictionary(order => order.Id);

        foreach (var id in listId)
        {
            var order = orders[orderIdByItemId[id]];

            if (!order.IsEditable)
                return Result<bool>.Validation("Order.NotEditable", $"Os itens do pedido {order.Id} só podem ser removidos enquanto ele estiver pendente.");

            repository.Remove(order.RemoveItem(id, settings));
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
