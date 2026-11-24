using MediatR;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListInternalEmployeesQueryHandler(
    IEntityRepository<Employee> repository)
    : IRequestHandler<ListInternalEmployeesQuery, List<StoreEmployeeResponse>>
{
    public async Task<List<StoreEmployeeResponse>> Handle(ListInternalEmployeesQuery request, CancellationToken ct)
    {
        return await repository.ListAsync(Entity => request.Ids.Contains(Entity.Id),
            Entity => new StoreEmployeeResponse(Entity.Id, Entity.EnterpriseId, Entity.FullName, Entity.IsActive), ct);
    }
}
