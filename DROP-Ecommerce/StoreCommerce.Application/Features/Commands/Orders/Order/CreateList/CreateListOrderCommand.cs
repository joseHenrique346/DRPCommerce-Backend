using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListOrderCommand(List<CreateOrderCommand> commands) : IRequest<Result<List<Order>>> { }
