using Commerce.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class InviteCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<InviteCommand, OneTimeLink>(context)
{
    protected override async Task<OneTimeLink> ExecuteAsync(InviteCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var email = UserEmail.Create(request.Email).Value.Trim().ToUpperInvariant();
        var roles = request.AccessRoleIds ?? [];

        if (request.KindId is not (2 or 3))
            throw new SecurityFailure(400, "Access.InvalidKind", "Convite deve ser para sócio ou funcionário.");

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        await ValidateRolesAsync(roles, tenant, ct);

        if (await data.EnterpriseMemberships.ExistsAsync(x => x.EnterpriseId == tenant && x.User.NormalizedEmail == email, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(409, "Access.MemberExists", "A pessoa já possui vínculo com esta loja. Altere o vínculo existente.");

        foreach (var pending in await data.EnterpriseInvitations.ListAsync(x => x.EnterpriseId == tenant && x.InvitedNormalizedEmail == email &&
            x.AcceptedAt == null && x.RevokedAt == null, ct, ignoreQueryFilters: true))
            pending.Revoke(Now);

        var raw = OpaqueToken.Create();
        var expires = Now.AddHours(Options.InvitationHours);
        var invitation = EnterpriseInvitation.Create(tenant, email, request.KindId, actor.MembershipId, OpaqueToken.Hash(raw), expires);

        foreach (var role in roles)
            invitation.AccessRoles.Add(InvitationAccessRole.Create(invitation, role));

        data.EnterpriseInvitations.Add(invitation);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);
        // Manual delivery by the principal owner for this stage: returned only once, never listed or logged.
        return new(invitation.Id, raw, expires);
    }
}
