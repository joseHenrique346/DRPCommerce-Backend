using StoreCommerce.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public class CustomerActionToken : BaseEntity, ITenantEntity
{
    public long CustomerId { get; private set; }
    public long EnterpriseId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    [JsonIgnore] public string TokenHash { get; private set; } = string.Empty;
    public AccountActionPurpose Purpose { get; private set; }
    public long SecurityVersionAtIssue { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    protected CustomerActionToken() { }

    public static CustomerActionToken Create(Customer account, string hash, AccountActionPurpose purpose, DateTime expiresAt) =>
        new() { CustomerId = account.Id, Customer = account, EnterpriseId = account.EnterpriseId,
            TokenHash = hash, Purpose = purpose, SecurityVersionAtIssue = account.SecurityVersion, ExpiresAt = expiresAt };

    public bool IsUsable(DateTime now, long version, AccountActionPurpose purpose) =>
        ConsumedAt is null && RevokedAt is null && ExpiresAt > now && SecurityVersionAtIssue == version && Purpose == purpose;
    public void Consume(DateTime now) => ConsumedAt = now;
    public void Revoke(DateTime now) => RevokedAt ??= now;
}
