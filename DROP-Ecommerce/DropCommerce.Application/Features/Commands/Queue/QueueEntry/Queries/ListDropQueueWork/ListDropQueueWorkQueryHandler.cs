using DropCommerce.Application.Interfaces.Reconciliation;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListDropQueueWorkQueryHandler(
    IDropReconciliationReader reader)
    : IRequestHandler<ListDropQueueWorkQuery, List<DropQueueWorkKey>>
{
    public async Task<List<DropQueueWorkKey>> Handle(ListDropQueueWorkQuery command, CancellationToken ct)
    {
        return await reader.ListQueueWorkAsync(command.Cursor, DateTime.UtcNow, ct);
    }
}
