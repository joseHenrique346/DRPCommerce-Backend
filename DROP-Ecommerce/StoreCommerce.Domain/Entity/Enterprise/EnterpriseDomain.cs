using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class EnterpriseDomain : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public string NormalizedHost { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; }
    [System.Text.Json.Serialization.JsonIgnore] public string? VerificationTokenHash { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    protected EnterpriseDomain() { }

    private EnterpriseDomain(long enterpriseId, string host, bool isPrimary, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));

        EnterpriseId = enterpriseId;
        UpdateDetails(host, isPrimary, isActive);
    }
    #endregion

    #region Functions
    public static EnterpriseDomain Create(long enterpriseId, string host, bool isPrimary, bool isActive)
    {
        return new EnterpriseDomain(enterpriseId, host, isPrimary, isActive);
    }

    public void UpdateDetails(string host, bool isPrimary, bool isActive)
    {
        BaseValidate.ValidateNotNullOrEmpty(host, nameof(Host));
        BaseValidate.ValidateMaxLength(host, 253, nameof(Host));

        if (NormalizedHost.Length > 0 && NormalizedHost != host.Trim().ToLowerInvariant())
        {
            IsVerified = false;
            VerificationTokenHash = null;
        }

        Host = host.Trim();
        NormalizedHost = Host.ToLowerInvariant();
        IsPrimary = isPrimary;
        IsActive = isActive;
    }

    public void SetVerificationChallenge(string hash) => VerificationTokenHash = hash;

    public void Verify()
    {
        IsVerified = true;
        VerificationTokenHash = null;
    }

    public void SetPublication(bool primary, bool active)
    {
        if (active && !IsVerified)
            throw new ArgumentException("Domínio precisa ser verificado antes da ativação.");

        IsPrimary = primary;
        IsActive = active;
    }
    #endregion
}