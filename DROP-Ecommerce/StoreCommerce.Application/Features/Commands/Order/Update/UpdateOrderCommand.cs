using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Atualiza status, cupom e endereço de entrega. Os valores do pedido são recalculados internamente.
/// </summary>
public record class UpdateOrderCommand(long id, long statusId, long paymentStatusId, long? couponId, string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes) : IRequest<Result<Order>> { }
