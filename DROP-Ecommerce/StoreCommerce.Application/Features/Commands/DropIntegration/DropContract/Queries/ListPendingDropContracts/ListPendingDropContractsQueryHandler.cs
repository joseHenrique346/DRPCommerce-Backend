using StoreCommerce.Application.Interfaces.Integrations;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListPendingDropContractsQueryHandler(
    IDropContractPublicationRepository persistence,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ListPendingDropContractsQuery, IReadOnlyList<DropCommercialRights>>(context)
{
    protected override async Task<IReadOnlyList<DropCommercialRights>> ExecuteAsync(ListPendingDropContractsQuery command, CancellationToken ct)
    {
        var changes = await persistence.PendingEnterprisesAsync(ct);

        return changes.Select(Rights).ToList();
    }
    private static DropCommercialRights Rights(Enterprise enterprise)
    {
        var rights = EnterpriseEntitlements.Resolve(enterprise.PlanId, enterprise.AddOnId);

        return new DropCommercialRights(enterprise.Id, enterprise.ContractRevision, enterprise.CatalogVersion,
            rights.AllowsDrop, rights.DropProductLimit);
    }
}
