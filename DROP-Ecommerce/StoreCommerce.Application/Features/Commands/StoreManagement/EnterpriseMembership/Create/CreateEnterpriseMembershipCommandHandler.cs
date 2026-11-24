using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateEnterpriseMembershipCommandHandler(
    IEnterpriseMembershipRepository repository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider)
    : IRequestHandler<CreateEnterpriseMembershipCommand, Result<EnterpriseMembership>>
{
    public async Task<Result<EnterpriseMembership>> Handle(CreateEnterpriseMembershipCommand request, CancellationToken ct)
    {
        var entity = EnterpriseMembership.Create(tenantProvider.GetEnterpriseId(), request.userId, request.kindId, request.isActive, request.joinedAt);
        await repository.AddRangeAsync([entity], ct);
        await unitOfWork.CommitAsync(ct);

        return Result<EnterpriseMembership>.Success(entity);
    }
}
