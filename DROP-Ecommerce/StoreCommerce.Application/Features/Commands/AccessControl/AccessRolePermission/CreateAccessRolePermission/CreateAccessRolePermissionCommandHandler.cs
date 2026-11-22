using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Interfaces.Persistence;
using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateAccessRolePermissionCommandHandler(IAccessRolePermissionRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : IRequestHandler<CreateAccessRolePermissionCommand, Result<AccessRolePermissionResponse>>
{
    public async Task<Result<AccessRolePermissionResponse>> Handle(CreateAccessRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var entity = AccessRolePermission.Create(tenantProvider.GetEnterpriseId(), request.accessRoleId, request.permissionId);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<AccessRolePermissionResponse>.Success(new(entity.EnterpriseId, entity.AccessRoleId, entity.PermissionId));
    }
}
