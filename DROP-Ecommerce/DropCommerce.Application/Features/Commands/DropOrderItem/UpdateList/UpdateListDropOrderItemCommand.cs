using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListDropOrderItemCommand(List<UpdateDropOrderItemCommand> commands) : IRequest<Result<List<DropOrderItem>>> { }
