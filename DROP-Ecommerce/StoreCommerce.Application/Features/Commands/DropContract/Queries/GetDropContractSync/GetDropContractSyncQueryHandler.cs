using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetDropContractSyncQueryHandler(
    IDropContractPublicationRepository persistence,
    IdentityHandlerContext context)
    : IdentityHandlerBase<GetDropContractSyncQuery, DropContractSyncResponse?>(context)
{
    protected override async Task<DropContractSyncResponse?> ExecuteAsync(GetDropContractSyncQuery command, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();

        return await persistence.StateAsync(actor.EnterpriseId, ct);
    }
}
