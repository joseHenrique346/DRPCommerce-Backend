using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListDropReservationCommand(List<UpdateDropReservationCommand> commands) : IRequest<Result<List<DropReservation>>> { }
