using StoreCommerce.Domain;

namespace StoreCommerce.Domain.Entity;

public class Document : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public long ReferenceId { get; private set; }
    public string ReferenceType { get; private set; }
    public long DocumentTypeId { get; private set; }
    public string Number { get; private set; }
    public string FileUrl { get; private set; }
    public long DocumentStatusId { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public Document() { }

    public Document(long enterpriseId, long referenceId, string referenceType, long documentTypeId, string number, string fileUrl, long documentStatusId, DateTime issuedAt, DateTime? expiresAt)
    {
        EnterpriseId = enterpriseId;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
        DocumentTypeId = documentTypeId;
        Number = number;
        FileUrl = fileUrl;
        DocumentStatusId = documentStatusId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }
}
