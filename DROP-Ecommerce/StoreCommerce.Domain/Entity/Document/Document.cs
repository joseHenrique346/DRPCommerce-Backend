using StoreCommerce.Domain;

namespace StoreCommerce.Domain.Entity;

public class Document : BaseEntity
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Document : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long ReferenceId { get; private set; }
    public string ReferenceType { get; private set; }
    public long DocumentTypeId { get; private set; }
    public string Number { get; private set; }
    public string FileUrl { get; private set; }
    public long DocumentStatusId { get; private set; }
    public long TypeId { get; private set; }
    public string Number { get; private set; }
    public string FileUrl { get; private set; }
    public long StatusId { get; private set; }
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
        TypeId = typeId;
        Number = number;
        FileUrl = fileUrl;
        StatusId = statusId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }
}
