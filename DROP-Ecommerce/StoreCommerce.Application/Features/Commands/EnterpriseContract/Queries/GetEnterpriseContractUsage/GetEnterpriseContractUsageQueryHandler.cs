using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetEnterpriseContractUsageQueryHandler(
    IdentityHandlerContext context,
    IEnterpriseContractUsageReader reader)
    : EnterpriseContractHandlerBase<GetEnterpriseContractUsageQuery, EnterpriseContractUsageResponse>(context)
{
    protected override async Task<EnterpriseContractUsageResponse> ExecuteAsync(
        GetEnterpriseContractUsageQuery command, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);

        var state = await ReadCurrentContractAsync(ct);
        var rights = EnterpriseEntitlements.Resolve(state.PlanId, state.AddOnId);
        var usage = await reader.ReadAsync(state.EnterpriseId, ct);

        return new EnterpriseContractUsageResponse(
            state.EnterpriseId,
            state.Revision,
            usage,
            rights.StoreProductLimit,
            Math.Max(0, rights.StoreProductLimit - usage.TotalStoreProducts),
            Math.Max(0, usage.TotalStoreProducts - rights.StoreProductLimit),
            rights.EmployeeLimit,
            Math.Max(0, rights.EmployeeLimit - usage.ActiveAdditionalMembers - usage.PendingInvitations),
            Math.Max(0, usage.ActiveAdditionalMembers - rights.EmployeeLimit),
            rights.DropProductLimit);
    }
}
