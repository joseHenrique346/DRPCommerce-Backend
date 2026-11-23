using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ListUnfinishedDropOrdersQuery(long Cursor) : IRequest<List<DropPendingOrder>>;
