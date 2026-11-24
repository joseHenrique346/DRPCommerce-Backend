using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

/// <summary>
/// Dados do envio. Obrigatórios em pedidos com produtos físicos: ShipmentTypeId, EstimatedDelivery e CarrierName
/// (dispensado na retirada no local).
/// </summary>
public sealed record ShipOrderRequest(long? ShipmentTypeId, string? CarrierName, string? TrackingCode, DateTime? EstimatedDelivery, string? Note);
