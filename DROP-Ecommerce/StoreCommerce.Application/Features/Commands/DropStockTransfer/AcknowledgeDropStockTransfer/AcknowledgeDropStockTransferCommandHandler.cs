using System.Text.Json;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AcknowledgeDropStockTransferCommandHandler(
    IDropStockTransferRepository persistence,
    IIdentityOperationScope identity,
    DropStockTransferScope transfers,
    IdentityHandlerContext context)
    : IdentityHandlerBase<AcknowledgeDropStockTransferCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(AcknowledgeDropStockTransferCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        Guid operationId = command.OperationId;
        long catalogId = command.CatalogProductId;

        var source = await persistence.ReadTransferAsync(operationId, tenant, ct);
        using var operation = identity.Begin();
        using var transfer = transfers.Begin(tenant, source.ProductId);
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, source.ProductId, ct);
        var row = await persistence.GetTransferAsync(source.Id, ct);
        row.Imported(catalogId, DateTime.UtcNow);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }

    private Task LockAsync(long tenant, long productId, CancellationToken ct)
    {
        return persistence.LockStockAsync(tenant, productId, ct);
    }

}
