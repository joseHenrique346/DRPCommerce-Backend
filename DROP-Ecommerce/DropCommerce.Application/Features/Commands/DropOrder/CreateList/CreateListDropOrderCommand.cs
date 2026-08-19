using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListDropOrderCommand(List<CreateDropOrderCommand> commands) : IRequest<Result<List<DropOrder>>> { }
