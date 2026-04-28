namespace StoreCommerce.Domain.Entity;

public class Shipment : BaseEntity
{
    public long OrderId { get; private set; }
    public long? SupplierId { get; private set; }
    public long TypeId { get; private set; }
    public string CarrierName { get; private set; }
    public string TrackingCode { get; private set; }
    public long StatusId { get; private set; }
    public decimal ShippingCost { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public DateTime EstimatedDelivery { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public Shipment() { }

    public Shipment(long orderId, long? supplierId, long typeId, string carrierName, string trackingCode, long statusId, decimal shippingCost, string addressLine, string city, string state, string zipCode, string country, DateTime estimatedDelivery, DateTime? shippedAt, DateTime? deliveredAt)
    {
        OrderId = orderId;
        SupplierId = supplierId;
        TypeId = typeId;
        CarrierName = carrierName;
        TrackingCode = trackingCode;
        StatusId = statusId;
        ShippingCost = shippingCost;
        AddressLine = addressLine;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        EstimatedDelivery = estimatedDelivery;
        ShippedAt = shippedAt;
        DeliveredAt = deliveredAt;
    }
}
