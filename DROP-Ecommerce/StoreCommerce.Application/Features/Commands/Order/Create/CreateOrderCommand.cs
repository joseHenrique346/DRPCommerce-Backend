using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateOrderCommand(long enterpriseId, long customerId, long? couponId, long statusId, long paymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string notes) : IRequest<Result<Order>> { }
