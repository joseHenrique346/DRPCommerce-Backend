using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetMembershipRoleCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<SetMembershipRoleCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(SetMembershipRoleCommand command, CancellationToken ct)
    {
        var membershipId = command.MembershipId;
        var roleId = command.RoleId;
        var grant = command.Grant;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var member = await data.EnterpriseMemberships.FindAsync(x => x.Id == membershipId && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();

        if (grant)
            await ValidateRolesAsync([roleId], tenant, ct);

        var relation = await data.MembershipAccessRoles.FindAsync(
            x => x.EnterpriseMembershipId == membershipId && x.AccessRoleId == roleId && x.EnterpriseId == tenant,
            ct,
            ignoreQueryFilters: true);

        if (grant && relation is null)
            data.MembershipAccessRoles.Add(MembershipAccessRole.Create(tenant, membershipId, roleId));

        if (!grant && relation is not null)
            data.MembershipAccessRoles.Remove(relation);

        member.InvalidateAccess();
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
