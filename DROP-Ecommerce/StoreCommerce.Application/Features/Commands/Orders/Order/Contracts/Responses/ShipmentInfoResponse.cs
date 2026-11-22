using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

public sealed record ShipmentInfoResponse(
    long Id, long ShipmentTypeId, string ShipmentType, long StatusId, string Status, string CarrierName, string TrackingCode,
    DateTime EstimatedDelivery, DateTime? ShippedAt, DateTime? DeliveredAt)
{
    public static ShipmentInfoResponse FromEntity(Shipment shipment) => new(
        shipment.Id, shipment.ShipmentTypeId,
        BaseStaticEntity.GetAll<ShipmentType>().First(type => type.Id == shipment.ShipmentTypeId).Description,
        shipment.ShipmentStatusId,
        BaseStaticEntity.GetAll<ShipmentStatus>().First(status => status.Id == shipment.ShipmentStatusId).Description,
        shipment.CarrierName, shipment.TrackingCode, shipment.EstimatedDelivery, shipment.ShippedAt, shipment.DeliveredAt);
}
