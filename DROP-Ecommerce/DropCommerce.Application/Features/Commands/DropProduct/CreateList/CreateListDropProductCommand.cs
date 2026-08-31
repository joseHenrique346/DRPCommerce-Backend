using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListDropProductCommand(List<CreateDropProductCommand> commands) : IRequest<Result<List<DropProduct>>> { }
