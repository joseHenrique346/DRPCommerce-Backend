using StoreCommerce.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public class UserActionToken : BaseEntity
{
    #region Properties
    public long UserId { get; private set; }
    [JsonIgnore] public string TokenHash { get; private set; } = string.Empty;
    public AccountActionPurpose Purpose { get; private set; }
    public long SecurityVersionAtIssue { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    #region Navigation Properties
    public User User { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    protected UserActionToken() { }
    #endregion

    #region Functions
    public static UserActionToken Create(User account, string hash, AccountActionPurpose purpose, DateTime expiresAt)
    {
        return new UserActionToken
        {
            UserId = account.Id,
            User = account,
            TokenHash = hash,
            Purpose = purpose,
            SecurityVersionAtIssue = account.SecurityVersion,
            ExpiresAt = expiresAt
        };
    }

    public bool IsUsable(DateTime now, long version, AccountActionPurpose purpose) =>
        ConsumedAt is null && RevokedAt is null && ExpiresAt > now && SecurityVersionAtIssue == version && Purpose == purpose;

    public void Consume(DateTime now) => ConsumedAt = now;

    public void Revoke(DateTime now) => RevokedAt ??= now;
    #endregion
}