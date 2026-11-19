using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetEnterpriseContractChangeQueryHandler(
    IdentityHandlerContext context,
    IEntityRepository<EnterpriseContractChange> changes)
    : EnterpriseContractHandlerBase<GetEnterpriseContractChangeQuery, EnterpriseContractChangeResponse>(context)
{
    protected override async Task<EnterpriseContractChangeResponse> ExecuteAsync(
        GetEnterpriseContractChangeQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var current = await ReadCurrentContractAsync(ct);
        var change = await changes.FindAsync(Change =>
            Change.Id == command.Id && Change.EnterpriseId == current.EnterpriseId,
            ct, ignoreQueryFilters: true, tracking: false) ?? throw NotFound();

        return EnterpriseContractResponseMapper.Change(change);
    }
}
