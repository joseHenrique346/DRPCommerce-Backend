using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RevokeOwnershipTransferCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RevokeOwnershipTransferCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(RevokeOwnershipTransferCommand command, CancellationToken ct)
    {
        var id = command.Id;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var transfer = await data.OwnershipTransfers.FindAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();
        transfer.Revoke(Now);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
