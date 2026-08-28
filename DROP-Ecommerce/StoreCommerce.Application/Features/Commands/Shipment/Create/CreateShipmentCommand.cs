using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateShipmentCommand(long orderId, long? supplierId, long typeId, string carrierName, string trackingCode, long statusId, decimal shippingCost, string addressLine, string city, string state, string zipCode, string country, DateTime estimatedDelivery, DateTime? shippedAt, DateTime? deliveredAt) : IRequest<Result<Shipment>> { }
