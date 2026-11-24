using MediatR;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetInternalEmployeeQueryHandler(
    IEntityRepository<Employee> repository)
    : IRequestHandler<GetInternalEmployeeQuery, StoreEmployeeResponse?>
{
    public async Task<StoreEmployeeResponse?> Handle(GetInternalEmployeeQuery request, CancellationToken ct)
    {
        return await repository.FindValueAsync(Entity => Entity.Id == request.Id,
            Entity => new StoreEmployeeResponse(Entity.Id, Entity.EnterpriseId, Entity.FullName, Entity.IsActive), ct);
    }
}
