using MediatR;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetInternalProductQueryHandler(
    IEntityRepository<Product> repository)
    : IRequestHandler<GetInternalProductQuery, StoreProductResponse?>
{
    public async Task<StoreProductResponse?> Handle(GetInternalProductQuery request, CancellationToken ct)
    {
        return await repository.FindValueAsync(Entity => Entity.Id == request.Id,
            Entity => new StoreProductResponse(Entity.Id, Entity.EnterpriseId, Entity.Name, Entity.SKU, Entity.IsActive, Entity.Price), ct);
    }
}
