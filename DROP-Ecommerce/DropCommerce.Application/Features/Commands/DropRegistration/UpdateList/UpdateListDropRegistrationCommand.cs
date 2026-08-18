using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListDropRegistrationCommand(List<UpdateDropRegistrationCommand> commands) : IRequest<Result<List<DropRegistration>>> { }
