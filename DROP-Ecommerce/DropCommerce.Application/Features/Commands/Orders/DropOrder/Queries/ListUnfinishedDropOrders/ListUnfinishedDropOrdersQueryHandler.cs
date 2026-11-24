using DropCommerce.Application.Interfaces.Reconciliation;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListUnfinishedDropOrdersQueryHandler(
    IDropReconciliationReader reader)
    : IRequestHandler<ListUnfinishedDropOrdersQuery, List<DropPendingOrder>>
{
    public async Task<List<DropPendingOrder>> Handle(ListUnfinishedDropOrdersQuery command, CancellationToken ct)
    {
        return await reader.ListUnfinishedOrdersAsync(command.Cursor, ct);
    }
}
