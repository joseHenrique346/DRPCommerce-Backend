using StoreCommerce.Domain.Interfaces.AccessControl;
using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByKeyAccessRolePermissionQueryHandler(IAccessRolePermissionRepository repository)
    : IRequestHandler<GetByKeyAccessRolePermissionQuery, Result<AccessRolePermissionResponse>>
{
    public async Task<Result<AccessRolePermissionResponse>> Handle(GetByKeyAccessRolePermissionQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByKeyAsync(request.accessRoleId, request.permissionId, cancellationToken);
        return entity is null
            ? Result<AccessRolePermissionResponse>.NotFound("AccessRolePermission.NotFound", "Relacionamento nao encontrado.")
            : Result<AccessRolePermissionResponse>.Success(new(entity.EnterpriseId, entity.AccessRoleId, entity.PermissionId));
    }
}
