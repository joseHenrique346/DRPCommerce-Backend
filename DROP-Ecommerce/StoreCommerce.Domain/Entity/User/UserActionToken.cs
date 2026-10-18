using StoreCommerce.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public class UserActionToken : BaseEntity
{
    public long UserId { get; private set; }

    public User User { get; private set; } = null!;
    [JsonIgnore] public string TokenHash { get; private set; } = string.Empty;
    public AccountActionPurpose Purpose { get; private set; }
    public long SecurityVersionAtIssue { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    protected UserActionToken() { }

    public static UserActionToken Create(User account, string hash, AccountActionPurpose purpose, DateTime expiresAt) =>
        new() { UserId = account.Id, User = account,
            TokenHash = hash, Purpose = purpose, SecurityVersionAtIssue = account.SecurityVersion, ExpiresAt = expiresAt };

    public bool IsUsable(DateTime now, long version, AccountActionPurpose purpose) =>
        ConsumedAt is null && RevokedAt is null && ExpiresAt > now && SecurityVersionAtIssue == version && Purpose == purpose;
    public void Consume(DateTime now) => ConsumedAt = now;
    public void Revoke(DateTime now) => RevokedAt ??= now;
}
