using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity.Invoice;

public class Invoice : BaseEntity
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Invoice : BaseEntity, ITenantEntity
{
    #region Properties
    public long OrderId { get; private set; }
    public long CustomerId { get; private set; }
    public long EnterpriseId { get; private set; }
    public string Number { get; private set; }
    public string Series { get; private set; }
    public string AccessKey { get; private set; }
    public long InvoiceTypeId { get; private set; }
    public long InvoiceStatusId { get; private set; }
    public long TypeId { get; private set; }
    public long StatusId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public string FileUrl { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    #endregion

    #region Constructor
    protected Invoice() { }

    private Invoice(long orderId, long customerId, long enterpriseId, string number, string series, string accessKey, long typeId, long statusId, decimal totalAmount, decimal taxAmount, string fileUrl, DateTime? issuedAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        EnterpriseId = enterpriseId;
        Number = number;
        Series = series;
        AccessKey = accessKey;
        InvoiceTypeId = invoiceTypeId;
        InvoiceStatusId = invoiceStatusId;
        TypeId = typeId;
        StatusId = statusId;
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        FileUrl = fileUrl;
        IssuedAt = issuedAt;
    }
    #endregion

    #region Functions
    public static Invoice Create(long orderId, long customerId, long enterpriseId, string number, string series, string accessKey, long typeId, long statusId, decimal totalAmount, decimal taxAmount, string fileUrl, DateTime? issuedAt)
    {
        BaseValidate.ValidatePositive(orderId, nameof(OrderId));
        BaseValidate.ValidatePositive(customerId, nameof(CustomerId));
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(number, nameof(Number));
        BaseValidate.ValidateMaxLength(number, 100, nameof(Number));
        BaseValidate.ValidateMaxLength(series, 50, nameof(Series));
        BaseValidate.ValidateMaxLength(accessKey, 255, nameof(AccessKey));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidatePositive(totalAmount, nameof(TotalAmount));
        BaseValidate.ValidatePositiveOrZero(taxAmount, nameof(TaxAmount));
        BaseValidate.ValidateMaxLength(fileUrl, 1000, nameof(FileUrl));
        BaseValidate.ValidateUrlFormat(fileUrl, nameof(FileUrl));
        BaseValidate.ValidateNullableNotFuture(issuedAt, nameof(IssuedAt));

        return new Invoice(orderId, customerId, enterpriseId, number, series, accessKey, typeId, statusId, totalAmount, taxAmount, fileUrl, issuedAt);
    }

    public void UpdateDetails(string number, string series, string accessKey, long typeId, long statusId, decimal totalAmount, decimal taxAmount, string fileUrl, DateTime? issuedAt)
    {
        BaseValidate.ValidateNotNullOrEmpty(number, nameof(Number));
        BaseValidate.ValidateMaxLength(number, 100, nameof(Number));
        BaseValidate.ValidateMaxLength(series, 50, nameof(Series));
        BaseValidate.ValidateMaxLength(accessKey, 255, nameof(AccessKey));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidatePositive(totalAmount, nameof(TotalAmount));
        BaseValidate.ValidatePositiveOrZero(taxAmount, nameof(TaxAmount));
        BaseValidate.ValidateMaxLength(fileUrl, 1000, nameof(FileUrl));
        BaseValidate.ValidateUrlFormat(fileUrl, nameof(FileUrl));
        BaseValidate.ValidateNullableNotFuture(issuedAt, nameof(IssuedAt));

        Number = number;
        Series = series;
        AccessKey = accessKey;
        TypeId = typeId;
        StatusId = statusId;
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        FileUrl = fileUrl;
        IssuedAt = issuedAt;
    }

    public void UpdateStatus(long statusId)
    {
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));

        StatusId = statusId;
    }
    #endregion
}
