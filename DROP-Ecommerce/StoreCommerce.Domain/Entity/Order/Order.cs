using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity.Order;

public class Order : BaseEntity
{
    #region Properties
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
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    #endregion

    #region Constructor
    protected Order() { }

    private Order(long enterpriseId, long customerId, long? couponId, long statusId, long paymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string notes)
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
    #endregion

    #region Functions
    public static Order Create(long enterpriseId, long customerId, long? couponId, long statusId, long paymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string notes)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(customerId, nameof(CustomerId));
        BaseValidate.ValidateNullablePositive(couponId, nameof(CouponId));
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidatePositive(paymentStatusId, nameof(PaymentStatusId));
        BaseValidate.ValidatePositive(subTotal, nameof(SubTotal));
        BaseValidate.ValidatePositiveOrZero(discountAmount, nameof(DiscountAmount));
        BaseValidate.ValidatePositiveOrZero(shippingCost, nameof(ShippingCost));
        BaseValidate.ValidatePositiveOrZero(taxAmount, nameof(TaxAmount));
        BaseValidate.ValidatePositive(totalAmount, nameof(TotalAmount));
        BaseValidate.ValidateMaxLength(shippingAddressLine, 500, nameof(ShippingAddressLine));
        BaseValidate.ValidateMaxLength(shippingCity, 255, nameof(ShippingCity));
        BaseValidate.ValidateMaxLength(shippingState, 255, nameof(ShippingState));
        BaseValidate.ValidateMaxLength(shippingZipCode, 20, nameof(ShippingZipCode));
        BaseValidate.ValidateMaxLength(notes, 2000, nameof(Notes));

        return new Order(enterpriseId, customerId, couponId, statusId, paymentStatusId, subTotal, discountAmount, shippingCost, taxAmount, totalAmount, shippingAddressLine, shippingCity, shippingState, shippingZipCode, notes);
    }

    public void UpdateStatus(long statusId, long paymentStatusId)
    {
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidatePositive(paymentStatusId, nameof(PaymentStatusId));

        StatusId = statusId;
        PaymentStatusId = paymentStatusId;
    }

    public void UpdateShippingAddress(string shippingAddressLine, string shippingCity, string shippingState, string shippingZipCode, string notes)
    {
        BaseValidate.ValidateMaxLength(shippingAddressLine, 500, nameof(ShippingAddressLine));
        BaseValidate.ValidateMaxLength(shippingCity, 255, nameof(ShippingCity));
        BaseValidate.ValidateMaxLength(shippingState, 255, nameof(ShippingState));
        BaseValidate.ValidateMaxLength(shippingZipCode, 20, nameof(ShippingZipCode));
        BaseValidate.ValidateMaxLength(notes, 2000, nameof(Notes));

        ShippingAddressLine = shippingAddressLine;
        ShippingCity = shippingCity;
        ShippingState = shippingState;
        ShippingZipCode = shippingZipCode;
        Notes = notes;
    }

    public void UpdateAmounts(decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount)
    {
        BaseValidate.ValidatePositive(subTotal, nameof(SubTotal));
        BaseValidate.ValidatePositiveOrZero(discountAmount, nameof(DiscountAmount));
        BaseValidate.ValidatePositiveOrZero(shippingCost, nameof(ShippingCost));
        BaseValidate.ValidatePositiveOrZero(taxAmount, nameof(TaxAmount));
        BaseValidate.ValidatePositive(totalAmount, nameof(TotalAmount));

        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        ShippingCost = shippingCost;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
    }
    #endregion
}
