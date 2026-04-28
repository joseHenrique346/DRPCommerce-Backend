namespace StoreCommerce.Domain.Entity;

public class Invoice : BaseEntity
{
    public long OrderId { get; private set; }
    public long CustomerId { get; private set; }
    public long EnterpriseId { get; private set; }
    public string Number { get; private set; }
    public string Series { get; private set; }
    public string AccessKey { get; private set; }
    public long TypeId { get; private set; }
    public long StatusId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public string FileUrl { get; private set; }
    public DateTime? IssuedAt { get; private set; }

    public Invoice() { }

    public Invoice(long orderId, long customerId, long enterpriseId, string number, string series, string accessKey, long typeId, long statusId, decimal totalAmount, decimal taxAmount, string fileUrl, DateTime? issuedAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        EnterpriseId = enterpriseId;
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
}
