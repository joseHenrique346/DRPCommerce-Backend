using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class Shipment : BaseEntity
{
    #region Properties
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
    #endregion

    #region Constructor
    protected Shipment() { }

    private Shipment(long orderId, long? supplierId, long typeId, string carrierName, string trackingCode, long statusId, decimal shippingCost, string addressLine, string city, string state, string zipCode, string country, DateTime estimatedDelivery, DateTime? shippedAt, DateTime? deliveredAt)
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
    #endregion

    #region Functions
    public static Shipment Create(long orderId, long? supplierId, long typeId, string carrierName, string trackingCode, long statusId, decimal shippingCost, string addressLine, string city, string state, string zipCode, string country, DateTime estimatedDelivery, DateTime? shippedAt, DateTime? deliveredAt)
    {
        BaseValidate.ValidatePositive(orderId, nameof(OrderId));
        BaseValidate.ValidateNullablePositive(supplierId, nameof(SupplierId));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidateMaxLength(carrierName, 255, nameof(CarrierName));
        BaseValidate.ValidateMaxLength(trackingCode, 255, nameof(TrackingCode));
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidatePositiveOrZero(shippingCost, nameof(ShippingCost));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidateMaxLength(state, 255, nameof(State));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));
        BaseValidate.ValidateNullableNotFuture(shippedAt, nameof(ShippedAt));
        BaseValidate.ValidateNullableNotFuture(deliveredAt, nameof(DeliveredAt));

        return new Shipment(orderId, supplierId, typeId, carrierName, trackingCode, statusId, shippingCost, addressLine, city, state, zipCode, country, estimatedDelivery, shippedAt, deliveredAt);
    }

    public void UpdateDetails(long? supplierId, long typeId, string carrierName, string trackingCode, decimal shippingCost, string addressLine, string city, string state, string zipCode, string country, DateTime estimatedDelivery)
    {
        BaseValidate.ValidateNullablePositive(supplierId, nameof(SupplierId));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidateMaxLength(carrierName, 255, nameof(CarrierName));
        BaseValidate.ValidateMaxLength(trackingCode, 255, nameof(TrackingCode));
        BaseValidate.ValidatePositiveOrZero(shippingCost, nameof(ShippingCost));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidateMaxLength(state, 255, nameof(State));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));

        SupplierId = supplierId;
        TypeId = typeId;
        CarrierName = carrierName;
        TrackingCode = trackingCode;
        ShippingCost = shippingCost;
        AddressLine = addressLine;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        EstimatedDelivery = estimatedDelivery;
    }

    public void UpdateStatus(long statusId)
    {
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));

        StatusId = statusId;
    }
    #endregion
}
