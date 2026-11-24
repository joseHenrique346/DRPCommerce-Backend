using MediatR;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetInternalEnterpriseQueryHandler(
    IEntityRepository<Enterprise> repository)
    : IRequestHandler<GetInternalEnterpriseQuery, StoreEnterpriseResponse?>
{
    public async Task<StoreEnterpriseResponse?> Handle(GetInternalEnterpriseQuery request, CancellationToken ct)
    {
        return await repository.FindValueAsync(Entity => Entity.Id == request.Id,
            Entity => new StoreEnterpriseResponse(Entity.Id, Entity.TradeName, Entity.IsActive, Entity.LegalName), ct);
    }
}
