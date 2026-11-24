using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ListMyDropOrdersQuery() : IRequest<List<DropOrderSummary>>;
