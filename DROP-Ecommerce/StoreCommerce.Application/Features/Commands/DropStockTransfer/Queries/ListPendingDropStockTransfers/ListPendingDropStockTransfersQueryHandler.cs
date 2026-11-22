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

public sealed class ListPendingDropStockTransfersQueryHandler(
    IDropStockTransferRepository persistence,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ListPendingDropStockTransfersQuery, IReadOnlyList<DropStockExport>>(context)
{
    protected override async Task<IReadOnlyList<DropStockExport>> ExecuteAsync(ListPendingDropStockTransfersQuery command, CancellationToken ct)
    {
        var rows = await persistence.PendingSnapshotsAsync(ct);

        return rows.Select(Row => JsonSerializer.Deserialize<DropStockExport>(Row)!).ToList();
    }
}
