using MediatR;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetInternalCustomerQueryHandler(
    IEntityRepository<Customer> repository)
    : IRequestHandler<GetInternalCustomerQuery, StoreCustomerResponse?>
{
    public async Task<StoreCustomerResponse?> Handle(GetInternalCustomerQuery request, CancellationToken ct)
    {
        return await repository.FindValueAsync(Entity => Entity.Id == request.Id,
            Entity => new StoreCustomerResponse(Entity.Id, Entity.EnterpriseId, Entity.FullName, Entity.IsActive), ct);
    }
}
