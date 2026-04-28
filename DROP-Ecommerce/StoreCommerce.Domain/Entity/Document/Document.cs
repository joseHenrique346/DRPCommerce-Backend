namespace StoreCommerce.Domain.Entity;

public class Document : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public long ReferenceId { get; private set; }
    public string ReferenceType { get; private set; }
    public long TypeId { get; private set; }
    public string Number { get; private set; }
    public string FileUrl { get; private set; }
    public long StatusId { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public Document() { }

    public Document(long enterpriseId, long referenceId, string referenceType, long typeId, string number, string fileUrl, long statusId, DateTime issuedAt, DateTime? expiresAt)
    {
        EnterpriseId = enterpriseId;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
        TypeId = typeId;
        Number = number;
        FileUrl = fileUrl;
        StatusId = statusId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }
}
