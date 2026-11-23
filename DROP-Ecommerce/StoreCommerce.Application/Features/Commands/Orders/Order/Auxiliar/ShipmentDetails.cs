using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Orders;

/// <summary>
/// Dados do envio. Sem transportadora na retirada no local (ShipmentType.Pickup).
/// </summary>
public sealed record ShipmentDetails(long ShipmentTypeId, string? CarrierName, string? TrackingCode, DateTime EstimatedDelivery);
