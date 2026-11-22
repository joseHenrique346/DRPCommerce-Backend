using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Integrations;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AcknowledgeDropContractCommandHandler(
    IDropContractPublicationRepository persistence,
    IIdentityOperationScope identity,
    DropContractAcknowledgmentScope acknowledgments,
    IdentityHandlerContext context)
    : IdentityHandlerBase<AcknowledgeDropContractCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(AcknowledgeDropContractCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        long revision = command.Revision;

        using var operation = identity.Begin();
        using var acknowledgment = acknowledgments.Begin();
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await data.Enterprises.LockAsync(tenant, ct);
        var publication = await persistence.Publications.FindAsync(Publication => Publication.EnterpriseId == tenant,
            ct, ignoreQueryFilters: true);

        if (publication is null || revision < 1 || revision > publication.Revision)
            return false;

        publication.Acknowledge(revision, DateTime.UtcNow);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
