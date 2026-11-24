using SharedCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Domain.StaticEntity;

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
    public Guid? PaymentOperationId { get; private set; }
    public long PaymentRevision { get; private set; }
    public bool PaymentIntentAccepted { get; private set; }
    public string? PaymentCheckoutUrl { get; private set; }
    public string? FinancialReviewCode { get; private set; }
    public string? BuyerNameSnapshot { get; private set; }
    public string? BuyerEmailSnapshot { get; private set; }
    public bool FulfillmentAcknowledged { get; private set; }
    public string? ProviderPaymentId { get; private set; }
    public string? PaymentResolution { get; private set; }

    public void RecordPaymentResolution(string status)
    {
        PaymentResolution = status;
    }

    public void RecordProviderPayment(string? paymentId)
    {
        if (paymentId is not null)
            ProviderPaymentId = paymentId;
    }

    public void PreparePayment(string buyerName, string buyerEmail)
    {
        PaymentOperationId ??= Guid.NewGuid();
        BuyerNameSnapshot = buyerName;
        BuyerEmailSnapshot = buyerEmail;
    }

    public void PaymentAccepted(string? checkoutUrl)
    {
        PaymentIntentAccepted = true;
        PaymentCheckoutUrl = checkoutUrl ?? PaymentCheckoutUrl;
    }

    public void ObservePayment(long revision, bool approved, string? review)
    {
        if (revision <= PaymentRevision)
            return;

        PaymentRevision = revision;

        if (review is not null)
        {
            FinancialReviewCode = review;

            return;
        }

        if (approved)
        {
            DropOrderPaymentStatusId = DropOrderPaymentStatus.Paid.Id;
            DropOrderStatusId = DropOrderStatus.Confirmed.Id;
        }
    }

    public void ExpireUnpaid()
    {
        if (DropOrderPaymentStatusId == DropOrderPaymentStatus.Pending.Id)
            DropOrderStatusId = DropOrderStatus.Cancelled.Id;
    }

    public void AdvanceFulfillment(long statusId)
    {
        if (DropOrderPaymentStatusId != DropOrderPaymentStatus.Paid.Id || FinancialReviewCode is not null || statusId < DropOrderStatusId || statusId is < 3 or > 5)
            throw new InvalidOperationException("Transição de entrega inválida.");

        DropOrderStatusId = statusId;
    }

    public void AcknowledgeFulfillment()
    {
        FulfillmentAcknowledged = true;
    }

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

    protected DropOrder()
    {
    }

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
