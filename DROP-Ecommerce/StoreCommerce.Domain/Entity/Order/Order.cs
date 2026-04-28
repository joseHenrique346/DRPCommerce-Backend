namespace StoreCommerce.Domain.Entity;

public class Order : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public long CustomerId { get; private set; }
    public long? CouponId { get; private set; }
    public long StatusId { get; private set; }
    public long PaymentStatusId { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string ShippingAddressLine { get; private set; }
    public string ShippingCity { get; private set; }
    public string ShippingState { get; private set; }
    public string ShippingZipCode { get; private set; }
    public string Notes { get; private set; }

    public Order() { }

    public Order(long enterpriseId, long customerId, long? couponId, long statusId, long paymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string notes)
    {
        EnterpriseId = enterpriseId;
        CustomerId = customerId;
        CouponId = couponId;
        StatusId = statusId;
        PaymentStatusId = paymentStatusId;
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        ShippingCost = shippingCost;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        ShippingAddressLine = shippingAddressLine;
        ShippingCity = shippingCity;
        ShippingState = shippingState;
        ShippingZipCode = shippingZipCode;
        Notes = notes;
    }
}
