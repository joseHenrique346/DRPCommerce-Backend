using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class EnterpriseDomain : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public string NormalizedHost { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; }
    public Enterprise Enterprise { get; private set; } = null!;

    protected EnterpriseDomain() { }

    private EnterpriseDomain(long enterpriseId, string host, bool isPrimary, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        EnterpriseId = enterpriseId;
        UpdateDetails(host, isPrimary, isActive);
    }

    public static EnterpriseDomain Create(long enterpriseId, string host, bool isPrimary, bool isActive) =>
        new(enterpriseId, host, isPrimary, isActive);

    public void UpdateDetails(string host, bool isPrimary, bool isActive)
    {
        BaseValidate.ValidateNotNullOrEmpty(host, nameof(Host));
        BaseValidate.ValidateMaxLength(host, 253, nameof(Host));
        Host = host.Trim();
        NormalizedHost = Host.ToLowerInvariant();
        IsPrimary = isPrimary;
        IsActive = isActive;
    }
}
