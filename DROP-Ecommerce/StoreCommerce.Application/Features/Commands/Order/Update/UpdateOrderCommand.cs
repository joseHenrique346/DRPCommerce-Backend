using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Atualiza cupom e endereço de entrega. O status muda apenas pelos fluxos do pedido (confirmar pagamento, enviar, cancelar...).
/// </summary>
public record class UpdateOrderCommand(long id, long? couponId, string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes) : IRequest<Result<Order>> { }
