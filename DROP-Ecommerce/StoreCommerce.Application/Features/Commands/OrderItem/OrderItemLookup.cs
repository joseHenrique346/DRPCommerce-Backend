using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

internal static class OrderItemLookup
{
    /// <summary>
    /// Retorna o pedido de cada item informado, respeitando o filtro de loja aplicado aos itens.
    /// </summary>
    public static async Task<Dictionary<long, long>> GetOrderIdsAsync(IRepository<OrderItem> repository, IReadOnlyCollection<long> itemIds, CancellationToken cancellationToken)
    {
        var rows = await repository.ListAsync(item => itemIds.Contains(item.Id), item => new { item.Id, item.OrderId }, cancellationToken);

        return rows.ToDictionary(row => row.Id, row => row.OrderId);
    }
}
