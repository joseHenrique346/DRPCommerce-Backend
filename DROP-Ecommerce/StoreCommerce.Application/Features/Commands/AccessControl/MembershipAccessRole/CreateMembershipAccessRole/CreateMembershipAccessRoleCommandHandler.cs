using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Interfaces.Persistence;
using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateMembershipAccessRoleCommandHandler(IMembershipAccessRoleRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : IRequestHandler<CreateMembershipAccessRoleCommand, Result<MembershipAccessRoleResponse>>
{
    public async Task<Result<MembershipAccessRoleResponse>> Handle(CreateMembershipAccessRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = MembershipAccessRole.Create(tenantProvider.GetEnterpriseId(), request.enterpriseMembershipId, request.accessRoleId);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<MembershipAccessRoleResponse>.Success(new(entity.EnterpriseId, entity.EnterpriseMembershipId, entity.AccessRoleId));
    }
}
