using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateDropOrderItemCommand(long id, long dropOrderId, long dropProductId, string itemName, string sku, int quantity, decimal unitPrice, decimal totalPrice) : IRequest<Result<DropOrderItem>> { }
