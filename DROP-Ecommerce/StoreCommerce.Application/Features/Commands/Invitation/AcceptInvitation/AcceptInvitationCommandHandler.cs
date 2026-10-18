using Commerce.Security;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AcceptInvitationCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<AcceptInvitationCommand, EnterpriseMembershipResponse>(context)
{
    protected override async Task<EnterpriseMembershipResponse> ExecuteAsync(AcceptInvitationCommand command, CancellationToken ct)
    {
        var token = command.Token;

        var userId = actor.UserId;
        var hash = OpaqueToken.Hash(token);
        var reference = await data.EnterpriseInvitations.FindAsync(x => x.TokenHash == hash, ct, ignoreQueryFilters: true, tracking: false) ?? throw InvalidLink();
        using var operation = scope.Begin();
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await data.Users.LockAsync(userId, ct);
        await LockEnterpriseAsync(reference.EnterpriseId, ct);
        var invitation = await data.GetInvitationWithRolesAsync(reference.Id, ct);
        var user = await data.Users.GetRequiredAsync(x => x.Id == userId, ct);

        if (!invitation.IsUsable(Now) || !user.IsActive || user.NormalizedEmail != invitation.InvitedNormalizedEmail ||
            !await data.EnterpriseMemberships.ExistsAsync(x => x.Id == invitation.IssuedByMembershipId &&
                x.EnterpriseId == invitation.EnterpriseId && x.KindId == 1 && x.IsActive && x.User.IsActive && x.Enterprise.IsActive, ct, ignoreQueryFilters: true))
            throw InvalidLink();

        var roleIds = invitation.AccessRoles.Select(x => x.AccessRoleId).ToList();
        await ValidateRolesAsync(roleIds, invitation.EnterpriseId, ct);

        if (await data.EnterpriseMemberships.ExistsAsync(x => x.UserId == userId && x.EnterpriseId == invitation.EnterpriseId, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(409, "Access.MemberExists", "O vínculo com esta loja já existe.");

        var membership = EnterpriseMembership.Create(invitation.EnterpriseId, userId, invitation.InvitedKindId, true, Now);
        data.EnterpriseMemberships.Add(membership);
        await unitOfWork.CommitAsync(ct);

        foreach (var roleId in roleIds)
            data.MembershipAccessRoles.Add(MembershipAccessRole.Create(invitation.EnterpriseId, membership.Id, roleId));

        invitation.Accept(Now);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return EnterpriseMembershipResponse.FromEntity(membership);
    }
}
