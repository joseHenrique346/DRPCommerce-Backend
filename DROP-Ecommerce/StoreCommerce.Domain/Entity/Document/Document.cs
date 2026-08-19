using StoreCommerce.Domain.Entity.Base;
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
    public DateTime IssuedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    protected Document() { }

    private Document(long enterpriseId, long referenceId, string referenceType, long documentTypeId, string number, string fileUrl, long documentStatusId, DateTime issuedAt, DateTime? expiresAt)
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

    public static Document Create(long enterpriseId, long referenceId, string referenceType, long documentTypeId, string number, string fileUrl, long documentStatusId, DateTime issuedAt, DateTime? expiresAt)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(referenceId, nameof(ReferenceId));
        BaseValidate.ValidateNotNullOrEmpty(referenceType, nameof(ReferenceType));
        BaseValidate.ValidateMaxLength(referenceType, 100, nameof(ReferenceType));
        BaseValidate.ValidatePositive(documentTypeId, nameof(DocumentTypeId));
        BaseValidate.ValidateMaxLength(number, 100, nameof(Number));
        BaseValidate.ValidateMaxLength(fileUrl, 1000, nameof(FileUrl));
        BaseValidate.ValidateUrlFormat(fileUrl, nameof(FileUrl));
        BaseValidate.ValidatePositive(documentStatusId, nameof(DocumentStatusId));
        BaseValidate.ValidateNotFuture(issuedAt, nameof(IssuedAt));
        BaseValidate.ValidateNullableGreaterThan(expiresAt, issuedAt, nameof(ExpiresAt));

        return new Document(enterpriseId, referenceId, referenceType, documentTypeId, number, fileUrl, documentStatusId, issuedAt, expiresAt);
    }

    public void UpdateDetails(long referenceId, string referenceType, long documentTypeId, string number, string fileUrl, long documentStatusId, DateTime issuedAt, DateTime? expiresAt)
    {
        BaseValidate.ValidatePositive(referenceId, nameof(ReferenceId));
        BaseValidate.ValidateNotNullOrEmpty(referenceType, nameof(ReferenceType));
        BaseValidate.ValidateMaxLength(referenceType, 100, nameof(ReferenceType));
        BaseValidate.ValidatePositive(documentTypeId, nameof(DocumentTypeId));
        BaseValidate.ValidateMaxLength(number, 100, nameof(Number));
        BaseValidate.ValidateMaxLength(fileUrl, 1000, nameof(FileUrl));
        BaseValidate.ValidateUrlFormat(fileUrl, nameof(FileUrl));
        BaseValidate.ValidatePositive(documentStatusId, nameof(DocumentStatusId));
        BaseValidate.ValidateNotFuture(issuedAt, nameof(IssuedAt));
        BaseValidate.ValidateNullableGreaterThan(expiresAt, issuedAt, nameof(ExpiresAt));

        ReferenceId = referenceId;
        ReferenceType = referenceType;
        DocumentTypeId = documentTypeId;
        Number = number;
        FileUrl = fileUrl;
        DocumentStatusId = documentStatusId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public void UpdateStatus(long documentStatusId)
    {
        BaseValidate.ValidatePositive(documentStatusId, nameof(DocumentStatusId));
        DocumentStatusId = documentStatusId;
    }
}
