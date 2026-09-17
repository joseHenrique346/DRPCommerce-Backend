using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListOrderItemCommand(List<UpdateOrderItemCommand> commands) : IRequest<Result<List<OrderItem>>> { }
