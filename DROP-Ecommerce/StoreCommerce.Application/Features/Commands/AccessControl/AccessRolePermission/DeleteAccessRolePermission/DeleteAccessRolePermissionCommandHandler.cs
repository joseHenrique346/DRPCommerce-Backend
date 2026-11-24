using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Interfaces.Persistence;
using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

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
