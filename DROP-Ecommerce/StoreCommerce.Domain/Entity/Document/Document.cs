using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Document : BaseEntity, ITenantEntity
{
    #region Properties
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
    #endregion

    #region Constructor
    protected Document() { }

    private Document(long enterpriseId, long referenceId, string referenceType, long typeId, string number, string fileUrl, long statusId, DateTime issuedAt, DateTime? expiresAt)
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
    #endregion

    #region Functions
    public static Document Create(long enterpriseId, long referenceId, string referenceType, long typeId, string number, string fileUrl, long statusId, DateTime issuedAt, DateTime? expiresAt)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(referenceId, nameof(ReferenceId));
        BaseValidate.ValidateNotNullOrEmpty(referenceType, nameof(ReferenceType));
        BaseValidate.ValidateMaxLength(referenceType, 100, nameof(ReferenceType));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidateMaxLength(number, 100, nameof(Number));
        BaseValidate.ValidateMaxLength(fileUrl, 1000, nameof(FileUrl));
        BaseValidate.ValidateUrlFormat(fileUrl, nameof(FileUrl));
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidateNotFuture(issuedAt, nameof(IssuedAt));
        BaseValidate.ValidateNullableGreaterThan(expiresAt, issuedAt, nameof(ExpiresAt));

        return new Document(enterpriseId, referenceId, referenceType, typeId, number, fileUrl, statusId, issuedAt, expiresAt);
    }

    public void UpdateDetails(long referenceId, string referenceType, long typeId, string number, string fileUrl, long statusId, DateTime issuedAt, DateTime? expiresAt)
    {
        BaseValidate.ValidatePositive(referenceId, nameof(ReferenceId));
        BaseValidate.ValidateNotNullOrEmpty(referenceType, nameof(ReferenceType));
        BaseValidate.ValidateMaxLength(referenceType, 100, nameof(ReferenceType));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidateMaxLength(number, 100, nameof(Number));
        BaseValidate.ValidateMaxLength(fileUrl, 1000, nameof(FileUrl));
        BaseValidate.ValidateUrlFormat(fileUrl, nameof(FileUrl));
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidateNotFuture(issuedAt, nameof(IssuedAt));
        BaseValidate.ValidateNullableGreaterThan(expiresAt, issuedAt, nameof(ExpiresAt));

        ReferenceId = referenceId;
        ReferenceType = referenceType;
        TypeId = typeId;
        Number = number;
        FileUrl = fileUrl;
        StatusId = statusId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public void UpdateStatus(long statusId)
    {
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));

        StatusId = statusId;
    }
    #endregion
}
