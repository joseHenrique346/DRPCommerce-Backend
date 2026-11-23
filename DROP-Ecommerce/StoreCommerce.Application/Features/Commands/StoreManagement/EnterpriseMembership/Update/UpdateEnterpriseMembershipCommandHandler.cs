using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateEnterpriseMembershipCommandHandler(
    IEnterpriseMembershipRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEnterpriseMembershipCommand, Result<EnterpriseMembership>>
{
    public async Task<Result<EnterpriseMembership>> Handle(UpdateEnterpriseMembershipCommand request, CancellationToken ct)
    {
        var entity = (await repository.GetListByListIdAsync([request.id], ct)).SingleOrDefault();

        if (entity is null)
            return Result<EnterpriseMembership>.NotFound("EnterpriseMembership.NotFound", $"EnterpriseMembership com id {request.id} não encontrado.");

        entity.UpdateDetails(request.kindId, request.isActive, request.joinedAt);
        repository.UpdateRange([entity]);
        await unitOfWork.CommitAsync(ct);

        return Result<EnterpriseMembership>.Success(entity);
    }
}
