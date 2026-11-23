using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateAccessRoleCommandHandler(
    IAccessRoleRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAccessRoleCommand, Result<AccessRole>>
{
    public async Task<Result<AccessRole>> Handle(UpdateAccessRoleCommand request, CancellationToken ct)
    {
        var entity = (await repository.GetListByListIdAsync([request.id], ct)).SingleOrDefault();

        if (entity is null)
            return Result<AccessRole>.NotFound("AccessRole.NotFound", $"AccessRole com id {request.id} não encontrado.");

        entity.UpdateDetails(request.name, request.description, request.isActive);
        repository.UpdateRange([entity]);
        await unitOfWork.CommitAsync(ct);

        return Result<AccessRole>.Success(entity);
    }
}
