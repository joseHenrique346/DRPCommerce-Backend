using DropCommerce.Domain.StaticEntity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Domain.Entity;

public class DropOrder : BaseEntity, ISoftDeletable
{
    #region Properties

    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public long DropReservationId { get; private set; }
    public long? DropCouponId { get; private set; }
    public long DropOrderStatusId { get; private set; }
    public long DropOrderPaymentStatusId { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string ShippingAddressLine { get; private set; }
    public string ShippingCity { get; private set; }
    public string ShippingState { get; private set; }
    public string ShippingZipCode { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }
    public DropReservation DropReservation { get; private set; }
    public DropCoupon? DropCoupon { get; private set; }
    public DropOrderStatus DropOrderStatus { get; private set; }
    public DropOrderPaymentStatus DropOrderPaymentStatus { get; private set; }
    public ICollection<DropOrderItem> ListDropOrderItem { get; private set; } = [];
    public ICollection<DropTransaction> ListDropTransaction { get; private set; } = [];

    #endregion

    #endregion

    #region Constructors

    protected DropOrder() { }

    private DropOrder(long dropEventId, long customerId, long dropReservationId, long? dropCouponId, long dropOrderStatusId, long dropOrderPaymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string? notes)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        DropReservationId = dropReservationId;
        DropCouponId = dropCouponId;
        DropOrderStatusId = dropOrderStatusId;
        DropOrderPaymentStatusId = dropOrderPaymentStatusId;
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

    public static DropOrder Create(long dropEventId, long customerId, long dropReservationId, long? dropCouponId, long dropOrderStatusId, long dropOrderPaymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string? notes)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateId(dropReservationId, nameof(dropReservationId));
        BaseValidate.ValidateIdNullable(dropCouponId, nameof(dropCouponId));
        BaseValidate.ValidateId(dropOrderStatusId, nameof(dropOrderStatusId));
        BaseValidate.ValidateId(dropOrderPaymentStatusId, nameof(dropOrderPaymentStatusId));
        BaseValidate.ValidatePositiveDecimal(subTotal, nameof(subTotal));
        BaseValidate.ValidatePositiveDecimal(discountAmount, nameof(discountAmount));
        BaseValidate.ValidatePositiveDecimal(shippingCost, nameof(shippingCost));
        BaseValidate.ValidatePositiveDecimal(taxAmount, nameof(taxAmount));
        BaseValidate.ValidatePositiveDecimal(totalAmount, nameof(totalAmount));
        BaseValidate.ValidateString(shippingAddressLine, nameof(shippingAddressLine));
        BaseValidate.ValidateString(shippingCity, nameof(shippingCity));
        BaseValidate.ValidateString(shippingState, nameof(shippingState));
        BaseValidate.ValidateString(shippingZipCode, nameof(shippingZipCode));

        return new DropOrder(dropEventId, customerId, dropReservationId, dropCouponId, dropOrderStatusId, dropOrderPaymentStatusId, subTotal, discountAmount, shippingCost, taxAmount, totalAmount, shippingAddressLine, shippingCity, shippingState, shippingZipCode, notes);
    }

    public void Update(long dropEventId, long customerId, long dropReservationId, long? dropCouponId, long dropOrderStatusId, long dropOrderPaymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string? notes)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateId(dropReservationId, nameof(dropReservationId));
        BaseValidate.ValidateIdNullable(dropCouponId, nameof(dropCouponId));
        BaseValidate.ValidateId(dropOrderStatusId, nameof(dropOrderStatusId));
        BaseValidate.ValidateId(dropOrderPaymentStatusId, nameof(dropOrderPaymentStatusId));
        BaseValidate.ValidatePositiveDecimal(subTotal, nameof(subTotal));
        BaseValidate.ValidatePositiveDecimal(discountAmount, nameof(discountAmount));
        BaseValidate.ValidatePositiveDecimal(shippingCost, nameof(shippingCost));
        BaseValidate.ValidatePositiveDecimal(taxAmount, nameof(taxAmount));
        BaseValidate.ValidatePositiveDecimal(totalAmount, nameof(totalAmount));
        BaseValidate.ValidateString(shippingAddressLine, nameof(shippingAddressLine));
        BaseValidate.ValidateString(shippingCity, nameof(shippingCity));
        BaseValidate.ValidateString(shippingState, nameof(shippingState));
        BaseValidate.ValidateString(shippingZipCode, nameof(shippingZipCode));

        DropEventId = dropEventId;
        CustomerId = customerId;
        DropReservationId = dropReservationId;
        DropCouponId = dropCouponId;
        DropOrderStatusId = dropOrderStatusId;
        DropOrderPaymentStatusId = dropOrderPaymentStatusId;
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

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    #endregion
}
