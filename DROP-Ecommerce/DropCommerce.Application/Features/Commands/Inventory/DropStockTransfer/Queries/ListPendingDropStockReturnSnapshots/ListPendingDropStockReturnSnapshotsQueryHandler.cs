using DropCommerce.Application.Interfaces.Reconciliation;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListPendingDropStockReturnSnapshotsQueryHandler(
    IDropReconciliationReader reader)
    : IRequestHandler<ListPendingDropStockReturnSnapshotsQuery, List<string>>
{
    public async Task<List<string>> Handle(ListPendingDropStockReturnSnapshotsQuery command, CancellationToken ct)
    {
        return await reader.ListPendingStockReturnSnapshotsAsync(ct);
    }
}
