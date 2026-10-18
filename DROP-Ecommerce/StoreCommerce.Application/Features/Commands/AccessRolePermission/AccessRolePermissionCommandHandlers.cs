using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateAccessRolePermissionCommandHandler(IAccessRolePermissionRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAccessRolePermissionCommand, Result<AccessRolePermissionResponse>>
{
    public async Task<Result<AccessRolePermissionResponse>> Handle(CreateAccessRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var entity = AccessRolePermission.Create(request.enterpriseId, request.accessRoleId, request.permissionId);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<AccessRolePermissionResponse>.Success(new(entity.EnterpriseId, entity.AccessRoleId, entity.PermissionId));
    }
}

public class DeleteAccessRolePermissionCommandHandler(IAccessRolePermissionRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAccessRolePermissionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteAccessRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByKeyAsync(request.accessRoleId, request.permissionId, cancellationToken);
        if (entity is null)
            return Result<bool>.NotFound("AccessRolePermission.NotFound", "Relacionamento nao encontrado.");
        repository.Delete(entity);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

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
