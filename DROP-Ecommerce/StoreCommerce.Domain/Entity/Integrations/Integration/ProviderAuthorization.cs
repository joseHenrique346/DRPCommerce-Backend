using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Domain.Entity;

public sealed class ProviderAuthorization : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long UserId { get; private set; }
    public long MembershipId { get; private set; }
    public long UserVersion { get; private set; }
    public string StateHash { get; private set; } = string.Empty;
    public string BrowserHash { get; private set; } = string.Empty;
    public string ProtectedVerifier { get; private set; } = string.Empty;
    public IntegrationEnvironment Environment { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }

    private ProviderAuthorization()
    {
    }

    public static ProviderAuthorization Create(long enterpriseId, long userId, long membershipId, long userVersion,
        string stateHash, string browserHash, string protectedVerifier, IntegrationEnvironment environment, DateTime now)
    {
        if (enterpriseId <= 0 || userId <= 0 || membershipId <= 0 || stateHash.Length != 64 || browserHash.Length != 64)
            throw new ArgumentException("Autorização inválida.");

        IntegrationProviders.Validate(IntegrationProviders.MercadoPago, environment);

        return new ProviderAuthorization
        {
            EnterpriseId = enterpriseId,
            UserId = userId,
            MembershipId = membershipId,
            UserVersion = userVersion,
            StateHash = stateHash,
            BrowserHash = browserHash,
            ProtectedVerifier = protectedVerifier,
            Environment = environment,
            ExpiresAt = now.AddMinutes(10)
        };
    }
}
