using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListDropProductCommand(List<UpdateDropProductCommand> commands) : IRequest<Result<List<DropProduct>>> { }
