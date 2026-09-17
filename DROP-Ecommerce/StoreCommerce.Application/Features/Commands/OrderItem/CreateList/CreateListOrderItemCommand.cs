using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListOrderItemCommand(List<CreateOrderItemCommand> commands) : IRequest<Result<List<OrderItem>>> { }
