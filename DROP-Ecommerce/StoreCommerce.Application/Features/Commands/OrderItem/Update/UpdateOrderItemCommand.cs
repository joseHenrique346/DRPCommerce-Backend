using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Altera a quantidade do item. Os valores do item e do pedido são recalculados internamente.
/// </summary>
public record class UpdateOrderItemCommand(long id, int quantity) : IRequest<Result<OrderItem>> { }
