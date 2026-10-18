using StoreCommerce.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public class CustomerRefreshToken : BaseEntity, ITenantEntity
{
    public long CustomerId { get; private set; }
    public long EnterpriseId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    [JsonIgnore] public string TokenHash { get; private set; } = string.Empty;
    public Guid FamilyId { get; private set; }
    public long SecurityVersionAtIssue { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime FamilyExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public long? ReplacedByTokenId { get; private set; }

    protected CustomerRefreshToken() { }

    public static CustomerRefreshToken Create(Customer account, string hash, Guid family, DateTime expiresAt) =>
        new()
        {
            CustomerId = account.Id, Customer = account, EnterpriseId = account.EnterpriseId,
            TokenHash = hash, FamilyId = family, SecurityVersionAtIssue = account.SecurityVersion,
            ExpiresAt = expiresAt, FamilyExpiresAt = expiresAt
        };

    public bool IsUsable(DateTime now, long version) =>
        ConsumedAt is null && RevokedAt is null && ExpiresAt > now && FamilyExpiresAt > now && SecurityVersionAtIssue == version;

    public void Consume(DateTime now) => ConsumedAt = now;
    public void SetReplacement(long id) => ReplacedByTokenId = id;
    public void Revoke(DateTime now) => RevokedAt ??= now;
}
