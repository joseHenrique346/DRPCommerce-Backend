using StoreCommerce.Domain.Interfaces.AccessControl;
using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllMembershipAccessRoleQueryHandler(IMembershipAccessRoleRepository repository)
    : IRequestHandler<GetAllMembershipAccessRoleQuery, Result<List<MembershipAccessRoleResponse>>>
{
    public async Task<Result<List<MembershipAccessRoleResponse>>> Handle(GetAllMembershipAccessRoleQuery request, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(cancellationToken);
        return Result<List<MembershipAccessRoleResponse>>.Success(entities
            .Select(entity => new MembershipAccessRoleResponse(entity.EnterpriseId, entity.EnterpriseMembershipId, entity.AccessRoleId)).ToList());
    }
}
