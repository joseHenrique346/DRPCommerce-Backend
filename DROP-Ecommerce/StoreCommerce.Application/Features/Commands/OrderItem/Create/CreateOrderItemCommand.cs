using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Adiciona um produto ou serviço ao pedido. Nome, SKU, preço e totais são obtidos do catálogo e calculados internamente.
/// </summary>
public record class CreateOrderItemCommand(long orderId, long? productId, long? serviceId, int quantity) : IRequest<Result<OrderItem>> { }
