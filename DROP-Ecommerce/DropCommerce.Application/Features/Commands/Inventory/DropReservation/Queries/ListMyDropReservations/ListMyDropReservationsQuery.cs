using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ListMyDropReservationsQuery() : IRequest<List<DropReservationSummary>>;
