using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class Shipment : BaseEntity
{
    public long OrderId { get; private set; }
    public long? SupplierId { get; private set; }
    public long ShipmentTypeId { get; private set; }
    public string CarrierName { get; private set; }
    public string TrackingCode { get; private set; }
    public long ShipmentStatusId { get; private set; }
    public decimal ShippingCost { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public long StateId { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public DateTime EstimatedDelivery { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    protected Shipment() { }

    private Shipment(long orderId, long? supplierId, long shipmentTypeId, string carrierName, string trackingCode, long shipmentStatusId, decimal shippingCost, string addressLine, string city, long stateId, string zipCode, string country, DateTime estimatedDelivery, DateTime? shippedAt, DateTime? deliveredAt)
    {
        OrderId = orderId;
        SupplierId = supplierId;
        ShipmentTypeId = shipmentTypeId;
        CarrierName = carrierName;
        TrackingCode = trackingCode;
        ShipmentStatusId = shipmentStatusId;
        ShippingCost = shippingCost;
        AddressLine = addressLine;
        City = city;
        StateId = stateId;
        ZipCode = zipCode;
        Country = country;
        EstimatedDelivery = estimatedDelivery;
        ShippedAt = shippedAt;
        DeliveredAt = deliveredAt;
    }

    public static Shipment Create(long orderId, long? supplierId, long shipmentTypeId, string carrierName, string trackingCode, long shipmentStatusId, decimal shippingCost, string addressLine, string city, long stateId, string zipCode, string country, DateTime estimatedDelivery, DateTime? shippedAt, DateTime? deliveredAt)
    {
        BaseValidate.ValidatePositive(orderId, nameof(OrderId));
        BaseValidate.ValidateNullablePositive(supplierId, nameof(SupplierId));
        BaseValidate.ValidatePositive(shipmentTypeId, nameof(ShipmentTypeId));
        BaseValidate.ValidateMaxLength(carrierName, 255, nameof(CarrierName));
        BaseValidate.ValidateMaxLength(trackingCode, 255, nameof(TrackingCode));
        BaseValidate.ValidatePositive(shipmentStatusId, nameof(ShipmentStatusId));
        BaseValidate.ValidatePositiveOrZero(shippingCost, nameof(ShippingCost));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));
        BaseValidate.ValidateNullableNotFuture(shippedAt, nameof(ShippedAt));
        BaseValidate.ValidateNullableNotFuture(deliveredAt, nameof(DeliveredAt));

        return new Shipment(orderId, supplierId, shipmentTypeId, carrierName, trackingCode, shipmentStatusId, shippingCost, addressLine, city, stateId, zipCode, country, estimatedDelivery, shippedAt, deliveredAt);
    }

    public void UpdateDetails(long? supplierId, long shipmentTypeId, string carrierName, string trackingCode, decimal shippingCost, string addressLine, string city, long stateId, string zipCode, string country, DateTime estimatedDelivery)
    {
        BaseValidate.ValidateNullablePositive(supplierId, nameof(SupplierId));
        BaseValidate.ValidatePositive(shipmentTypeId, nameof(ShipmentTypeId));
        BaseValidate.ValidateMaxLength(carrierName, 255, nameof(CarrierName));
        BaseValidate.ValidateMaxLength(trackingCode, 255, nameof(TrackingCode));
        BaseValidate.ValidatePositiveOrZero(shippingCost, nameof(ShippingCost));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));

        SupplierId = supplierId;
        ShipmentTypeId = shipmentTypeId;
        CarrierName = carrierName;
        TrackingCode = trackingCode;
        ShippingCost = shippingCost;
        AddressLine = addressLine;
        City = city;
        StateId = stateId;
        ZipCode = zipCode;
        Country = country;
        EstimatedDelivery = estimatedDelivery;
    }

    public void UpdateStatus(long shipmentStatusId)
    {
        BaseValidate.ValidatePositive(shipmentStatusId, nameof(ShipmentStatusId));
        ShipmentStatusId = shipmentStatusId;
    }
}
