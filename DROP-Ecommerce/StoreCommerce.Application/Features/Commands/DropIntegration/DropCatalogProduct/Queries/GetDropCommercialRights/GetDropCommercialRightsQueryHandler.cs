using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetDropCommercialRightsQueryHandler(
    IdentityHandlerContext context)
    : IdentityHandlerBase<GetDropCommercialRightsQuery, DropCommercialRights?>(context)
{
    protected override async Task<DropCommercialRights?> ExecuteAsync(GetDropCommercialRightsQuery command, CancellationToken ct)
    {
        long tenant = command.Tenant;

        var enterprise = await data.Enterprises.FindAsync(Enterprise => Enterprise.Id == tenant && Enterprise.IsActive,
            ct, ignoreQueryFilters: true, tracking: false);

        return enterprise is null ? null : Rights(enterprise);
    }

    private static DropCommercialRights Rights(Enterprise enterprise)
    {
        var rights = EnterpriseEntitlements.Resolve(enterprise.PlanId, enterprise.AddOnId);

        return new DropCommercialRights(enterprise.Id, enterprise.ContractRevision, enterprise.CatalogVersion,
            rights.AllowsDrop, rights.DropProductLimit);
    }
}
