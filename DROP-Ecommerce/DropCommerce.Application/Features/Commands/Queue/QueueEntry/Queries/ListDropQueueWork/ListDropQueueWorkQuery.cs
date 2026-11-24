using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ListDropQueueWorkQuery(long Cursor) : IRequest<List<DropQueueWorkKey>>;
