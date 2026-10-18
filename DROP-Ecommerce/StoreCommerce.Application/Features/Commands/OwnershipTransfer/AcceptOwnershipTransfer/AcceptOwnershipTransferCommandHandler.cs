using Commerce.Security;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AcceptOwnershipTransferCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<AcceptOwnershipTransferCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(AcceptOwnershipTransferCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var userId = actor.UserId;
        var hash = OpaqueToken.Hash(request.Token);
        var reference = await data.OwnershipTransfers.FindAsync(x => x.TokenHash == hash, ct, ignoreQueryFilters: true, tracking: false) ?? throw InvalidLink();
        using var operation = scope.Begin();
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await data.Users.LockAsync(userId, ct);
        await LockEnterpriseAsync(reference.EnterpriseId, ct);
        var transfer = await data.OwnershipTransfers.GetRequiredAsync(x => x.Id == reference.Id, ct, ignoreQueryFilters: true);
        var recipient = await data.EnterpriseMemberships.GetRequiredAsync(
            x => x.Id == transfer.RecipientMembershipId && x.EnterpriseId == transfer.EnterpriseId,
            ct,
            ignoreQueryFilters: true);
        var issuer = await data.EnterpriseMemberships.GetRequiredAsync(
            x => x.Id == transfer.IssuedByMembershipId && x.EnterpriseId == transfer.EnterpriseId,
            ct,
            ignoreQueryFilters: true);
        var user = await data.Users.GetRequiredAsync(x => x.Id == userId, ct);

        if (!transfer.IsUsable(Now) || recipient.UserId != userId || !recipient.IsActive || !user.IsActive || !issuer.IsActive || issuer.KindId != 1 ||
            !await data.Enterprises.ExistsAsync(x => x.Id == transfer.EnterpriseId && x.IsActive, ct, ignoreQueryFilters: true) ||
            !await data.Users.ExistsAsync(x => x.Id == issuer.UserId && x.IsActive, ct))
            throw InvalidLink();

        if (!passwords.Verify(user, request.CurrentPassword, out _))
            throw new SecurityFailure(401, "Identity.InvalidCredentials", "Credenciais inválidas.");
        // Two statements within one transaction: release the partial unique index before promoting the recipient.
        issuer.UpdateDetails(2, true, issuer.JoinedAt);
        await unitOfWork.CommitAsync(ct);
        recipient.UpdateDetails(1, true, recipient.JoinedAt);
        transfer.Accept(Now);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
