using DropCommerce.Application.Features.Commands;

namespace DropCommerce.Application.Interfaces.Reconciliation;

public interface IDropReconciliationReader
{
    Task<List<DropQueueWorkKey>> ListQueueWorkAsync(long cursor, DateTime now, CancellationToken ct);
    Task<List<string>> ListPendingStockReturnSnapshotsAsync(CancellationToken ct);
    Task<List<DropPendingOrder>> ListUnfinishedOrdersAsync(long orderCursor, CancellationToken ct);
}
