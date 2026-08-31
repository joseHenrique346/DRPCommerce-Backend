using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Order : BaseEntity, ISoftDeletable, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long CustomerId { get; private set; }
    public long? CouponId { get; private set; }
    public long OrderStatusId { get; private set; }
    public long OrderPaymentStatusId { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string ShippingAddressLine { get; private set; }
    public string ShippingCity { get; private set; }
    public long ShippingStateId { get; private set; }
    public string ShippingZipCode { get; private set; }
    public string Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    #endregion

    #region Constructor
    protected Order() { }

    private Order(long enterpriseId, long customerId, long? couponId, long orderStatusId, long orderPaymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes)
    {
        EnterpriseId = enterpriseId;
        CustomerId = customerId;
        CouponId = couponId;
        OrderStatusId = orderStatusId;
        OrderPaymentStatusId = orderPaymentStatusId;
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        ShippingCost = shippingCost;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        ShippingAddressLine = shippingAddressLine;
        ShippingCity = shippingCity;
        ShippingStateId = shippingStateId;
        ShippingZipCode = shippingZipCode;
        Notes = notes;
    }
    #endregion

    #region Functions
    public static Order Create(long enterpriseId, long customerId, long? couponId, long orderStatusId, long orderPaymentStatusId, decimal subTotal, decimal discountAmount, decimal shippingCost, decimal taxAmount, decimal totalAmount, string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(customerId, nameof(CustomerId));
        BaseValidate.ValidateNullablePositive(couponId, nameof(CouponId));
        BaseValidate.ValidatePositive(orderStatusId, nameof(OrderStatusId));
        BaseValidate.ValidatePositive(orderPaymentStatusId, nameof(OrderPaymentStatusId));
        BaseValidate.ValidatePositive(subTotal, nameof(SubTotal));
        BaseValidate.ValidatePositiveOrZero(discountAmount, nameof(DiscountAmount));
        BaseValidate.ValidatePositiveOrZero(shippingCost, nameof(ShippingCost));
        BaseValidate.ValidatePositiveOrZero(taxAmount, nameof(TaxAmount));
        BaseValidate.ValidatePositive(totalAmount, nameof(TotalAmount));
        BaseValidate.ValidateMaxLength(shippingAddressLine, 500, nameof(ShippingAddressLine));
        BaseValidate.ValidateMaxLength(shippingCity, 255, nameof(ShippingCity));
        BaseValidate.ValidatePositive(shippingStateId, nameof(ShippingStateId));
        BaseValidate.ValidateMaxLength(shippingZipCode, 20, nameof(ShippingZipCode));
        BaseValidate.ValidateMaxLength(notes, 2000, nameof(Notes));

        return new Order(enterpriseId, customerId, couponId, orderStatusId, orderPaymentStatusId, subTotal, discountAmount, shippingCost, taxAmount, totalAmount, shippingAddressLine, shippingCity, shippingStateId, shippingZipCode, notes);
    }

    public void UpdateStatus(long orderStatusId, long orderPaymentStatusId)
    {
        BaseValidate.ValidatePositive(orderStatusId, nameof(OrderStatusId));
        BaseValidate.ValidatePositive(orderPaymentStatusId, nameof(OrderPaymentStatusId));

        OrderStatusId = orderStatusId;
        OrderPaymentStatusId = orderPaymentStatusId;
    }

    public void UpdateShippingAddress(string shippingAddressLine, string shippingCity, long shippingStateId, string shippingZipCode, string notes)
    {
        BaseValidate.ValidateMaxLength(shippingAddressLine, 500, nameof(ShippingAddressLine));
        BaseValidate.ValidateMaxLength(shippingCity, 255, nameof(ShippingCity));
        BaseValidate.ValidatePositive(shippingStateId, nameof(ShippingStateId));
        BaseValidate.ValidateMaxLength(shippingZipCode, 20, nameof(ShippingZipCode));
        BaseValidate.ValidateMaxLength(notes, 2000, nameof(Notes));

        ShippingAddressLine = shippingAddressLine;
        ShippingCity = shippingCity;
        ShippingStateId = shippingStateId;
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

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    #endregion
}
