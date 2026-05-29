using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListDropEventCommand(List<CreateDropEventCommand> commands) : IRequest<Result<List<DropEvent>>> { }
