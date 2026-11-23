using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record GetMyDropReservationQuery(long Id) : IRequest<DropReservationSummary?>;
