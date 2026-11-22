using StoreCommerce.Domain.Interfaces.AccessControl;
using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllAccessRolePermissionQueryHandler(IAccessRolePermissionRepository repository)
    : IRequestHandler<GetAllAccessRolePermissionQuery, Result<List<AccessRolePermissionResponse>>>
{
    public async Task<Result<List<AccessRolePermissionResponse>>> Handle(GetAllAccessRolePermissionQuery request, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(cancellationToken);
        return Result<List<AccessRolePermissionResponse>>.Success(entities
            .Select(entity => new AccessRolePermissionResponse(entity.EnterpriseId, entity.AccessRoleId, entity.PermissionId)).ToList());
    }
}
