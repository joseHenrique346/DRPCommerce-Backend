using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListDropReservationCommand(List<CreateDropReservationCommand> commands) : IRequest<Result<List<DropReservation>>> { }
