using Commerce.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RequestOwnershipTransferCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RequestOwnershipTransferCommand, OneTimeLink>(context)
{
    protected override async Task<OneTimeLink> ExecuteAsync(RequestOwnershipTransferCommand command, CancellationToken ct)
    {
        var request = command.Request;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var issuer = await data.Users.GetRequiredAsync(x => x.Id == actor.UserId, ct);

        if (!passwords.Verify(issuer, request.CurrentPassword, out _))
            throw new SecurityFailure(401, "Identity.InvalidCredentials", "Credenciais inválidas.");

        var recipient = await data.EnterpriseMemberships.FindAsync(x => x.Id == request.RecipientMembershipId &&
            x.EnterpriseId == tenant && x.IsActive && x.User.IsActive && x.KindId != 1, ct, ignoreQueryFilters: true) ?? throw NotFound();

        foreach (var old in await data.OwnershipTransfers.ListAsync(
            x => x.EnterpriseId == tenant && x.AcceptedAt == null && x.RevokedAt == null,
            ct,
            ignoreQueryFilters: true))
            old.Revoke(Now);

        var raw = OpaqueToken.Create();
        var expires = Now.AddMinutes(Options.OwnershipTransferMinutes);
        var transfer = OwnershipTransfer.Create(tenant, actor.MembershipId, recipient.Id, OpaqueToken.Hash(raw), expires);
        data.OwnershipTransfers.Add(transfer);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return new(transfer.Id, raw, expires);
    }
}
