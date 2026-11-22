using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AssignRolesCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<AssignRolesCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(AssignRolesCommand command, CancellationToken ct)
    {
        var membershipId = command.MembershipId;
        var request = command.Request;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var ids = request.AccessRoleIds ?? [];
        await ValidateRolesAsync(ids, tenant, ct);
        var member = await data.EnterpriseMemberships.FindAsync(x => x.Id == membershipId && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();
        var current = await data.MembershipAccessRoles.ListAsync(
            x => x.EnterpriseMembershipId == membershipId && x.EnterpriseId == tenant,
            ct,
            ignoreQueryFilters: true);
        data.MembershipAccessRoles.RemoveRange(current.Where(x => !ids.Contains(x.AccessRoleId)));

        foreach (var role in ids.Except(current.Select(x => x.AccessRoleId)))
            data.MembershipAccessRoles.Add(MembershipAccessRole.Create(tenant, membershipId, role));

        member.InvalidateAccess();
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
