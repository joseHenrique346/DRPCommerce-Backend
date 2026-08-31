using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListDropRegistrationCommand(List<CreateDropRegistrationCommand> commands) : IRequest<Result<List<DropRegistration>>> { }
