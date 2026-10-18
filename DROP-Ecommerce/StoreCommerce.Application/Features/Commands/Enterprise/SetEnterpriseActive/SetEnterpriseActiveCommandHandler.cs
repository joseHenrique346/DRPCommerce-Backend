using Commerce.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetEnterpriseActiveCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<SetEnterpriseActiveCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(SetEnterpriseActiveCommand command, CancellationToken ct)
    {
        var enterpriseId = command.EnterpriseId;
        var active = command.Active;

        var userId = actor.UserId;
        var tenant = enterpriseId;

        if (tenant < 1)
            throw new SecurityFailure(400, "Enterprise.InvalidId", "Loja inválida.");

        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await LockUserAsync(userId, ct);
        await data.Enterprises.LockAsync(tenant, ct);
        var membership = await data.EnterpriseMemberships.FindAsync(
            x => x.EnterpriseId == tenant && x.UserId == userId && x.IsActive && x.KindId == 1 && x.User.IsActive,
            ct,
            ignoreQueryFilters: true)
            ?? throw new SecurityFailure(403, "Enterprise.PrimaryOwnerRequired", "Apenas o dono principal pode alterar a loja.");
        var enterprise = await data.Enterprises.GetRequiredAsync(x => x.Id == tenant, ct, ignoreQueryFilters: true);
        enterprise.SetActive(active);
        membership.InvalidateAccess();
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
