using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListOrderCommand(List<UpdateOrderCommand> commands) : IRequest<Result<List<Order>>> { }
