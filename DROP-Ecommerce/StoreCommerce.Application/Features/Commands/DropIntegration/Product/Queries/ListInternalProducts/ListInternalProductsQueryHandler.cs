using MediatR;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListInternalProductsQueryHandler(
    IEntityRepository<Product> repository)
    : IRequestHandler<ListInternalProductsQuery, List<StoreProductResponse>>
{
    public async Task<List<StoreProductResponse>> Handle(ListInternalProductsQuery request, CancellationToken ct)
    {
        return await repository.ListAsync(Entity => request.Ids.Contains(Entity.Id),
            Entity => new StoreProductResponse(Entity.Id, Entity.EnterpriseId, Entity.Name, Entity.SKU, Entity.IsActive, Entity.Price), ct);
    }
}
