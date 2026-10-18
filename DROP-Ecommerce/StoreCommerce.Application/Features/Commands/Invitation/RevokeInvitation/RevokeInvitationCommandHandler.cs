using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RevokeInvitationCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RevokeInvitationCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(RevokeInvitationCommand command, CancellationToken ct)
    {
        var id = command.Id;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var invitation = await data.EnterpriseInvitations.FindAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();
        invitation.Revoke(Now);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
