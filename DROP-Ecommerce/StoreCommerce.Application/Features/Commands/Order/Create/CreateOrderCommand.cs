using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Cria um pedido pendente. Subtotal, desconto e total são calculados a partir dos itens e do cupom.
/// </summary>
public record class CreateOrderCommand(long customerId, long? couponId, string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes) : IRequest<Result<Order>> { }
