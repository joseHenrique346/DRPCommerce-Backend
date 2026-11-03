using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ChangeEnterprisePlanCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<ChangeEnterprisePlanCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(ChangeEnterprisePlanCommand command, CancellationToken ct)
    {
        var planId = command.PlanId;

        if (EnterprisePlan.FromId(planId).Id != planId)
            throw new SecurityFailure(400, "Enterprise.InvalidPlan", "Plano invalido ou indisponivel.");

        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await LockEnterpriseAsync(tenant, ct);

        if (!await data.EnterpriseMemberships.ExistsAsync(x => x.Id == actor.MembershipId && x.UserId == actor.UserId &&
            x.EnterpriseId == tenant && x.IsActive && x.KindId == 1 && x.User.IsActive, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(403, "Enterprise.PrimaryOwnerRequired", "Apenas o dono principal pode alterar o plano da loja.");

        var enterprise = await data.Enterprises.GetRequiredAsync(x => x.Id == tenant, ct, ignoreQueryFilters: true);
        enterprise.ChangePlan(planId);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
