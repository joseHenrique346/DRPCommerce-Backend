using MediatR;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListInternalCustomersQueryHandler(
    IEntityRepository<Customer> repository)
    : IRequestHandler<ListInternalCustomersQuery, List<StoreCustomerResponse>>
{
    public async Task<List<StoreCustomerResponse>> Handle(ListInternalCustomersQuery request, CancellationToken ct)
    {
        return await repository.ListAsync(Entity => request.Ids.Contains(Entity.Id),
            Entity => new StoreCustomerResponse(Entity.Id, Entity.EnterpriseId, Entity.FullName, Entity.IsActive), ct);
    }
}
