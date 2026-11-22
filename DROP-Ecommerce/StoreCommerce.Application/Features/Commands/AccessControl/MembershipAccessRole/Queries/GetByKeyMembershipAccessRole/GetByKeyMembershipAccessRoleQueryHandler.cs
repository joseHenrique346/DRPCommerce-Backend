using StoreCommerce.Domain.Interfaces.AccessControl;
using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByKeyMembershipAccessRoleQueryHandler(IMembershipAccessRoleRepository repository)
    : IRequestHandler<GetByKeyMembershipAccessRoleQuery, Result<MembershipAccessRoleResponse>>
{
    public async Task<Result<MembershipAccessRoleResponse>> Handle(GetByKeyMembershipAccessRoleQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByKeyAsync(request.enterpriseMembershipId, request.accessRoleId, cancellationToken);
        return entity is null
            ? Result<MembershipAccessRoleResponse>.NotFound("MembershipAccessRole.NotFound", "Relacionamento nao encontrado.")
            : Result<MembershipAccessRoleResponse>.Success(new(entity.EnterpriseId, entity.EnterpriseMembershipId, entity.AccessRoleId));
    }
}
