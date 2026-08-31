using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateOrderItemCommand(long id, long orderId, long? productId, long? serviceId, string itemName, string sku, int quantity, decimal unitPrice, decimal discountAmount, decimal totalPrice) : IRequest<Result<OrderItem>> { }
