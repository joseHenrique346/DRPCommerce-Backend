namespace DropCommerce.Domain.Entity;

public class DropOrder : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public long ReservationId { get; private set; }
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

    #endregion

    #region Constructors

    protected DropOrder() { }

    private DropOrder(long dropEventId, long customerId, long reservationId, long? couponId, long statusId, long paymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string notes)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        ReservationId = reservationId;
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

    #endregion

    #region Functions

    public static DropOrder Create(long dropEventId, long customerId, long reservationId, long? couponId, long statusId, long paymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string notes)
    {
        return new DropOrder(dropEventId, customerId, reservationId, couponId, statusId, paymentStatusId, subTotal, discountAmount, shippingCost, taxAmount, totalAmount, shippingAddressLine, shippingCity, shippingState, shippingZipCode, notes);
    }

    #endregion
}