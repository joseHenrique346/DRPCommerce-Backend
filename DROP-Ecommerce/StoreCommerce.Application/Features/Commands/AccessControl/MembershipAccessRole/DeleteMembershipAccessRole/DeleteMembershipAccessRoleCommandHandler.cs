using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Interfaces.Persistence;
using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteMembershipAccessRoleCommandHandler(IMembershipAccessRoleRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteMembershipAccessRoleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteMembershipAccessRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByKeyAsync(request.enterpriseMembershipId, request.accessRoleId, cancellationToken);
        if (entity is null)
            return Result<bool>.NotFound("MembershipAccessRole.NotFound", "Relacionamento nao encontrado.");
        repository.Delete(entity);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
