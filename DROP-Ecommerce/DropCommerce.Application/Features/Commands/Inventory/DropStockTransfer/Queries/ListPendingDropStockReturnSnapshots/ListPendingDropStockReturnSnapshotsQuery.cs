using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ListPendingDropStockReturnSnapshotsQuery() : IRequest<List<string>>;
