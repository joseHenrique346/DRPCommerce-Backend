using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ListMyDropWaitlistEntriesQuery() : IRequest<List<DropWaitlistEntrySummary>>;
