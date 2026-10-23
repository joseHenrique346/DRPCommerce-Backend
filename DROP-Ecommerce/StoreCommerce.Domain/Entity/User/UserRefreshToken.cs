using StoreCommerce.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public class UserRefreshToken : BaseEntity
{
    #region Properties
    public long UserId { get; private set; }
    public long? EnterpriseMembershipId { get; private set; }
    [JsonIgnore] public string TokenHash { get; private set; } = string.Empty;
    public Guid FamilyId { get; private set; }
    public long SecurityVersionAtIssue { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime FamilyExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public long? ReplacedByTokenId { get; private set; }

    #region Navigation Properties
    public User User { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    protected UserRefreshToken() { }
    #endregion

    #region Functions
    public static UserRefreshToken Create(User account, string hash, Guid family, DateTime expiresAt, long? membershipId = null)
    {
        return new UserRefreshToken
        {
            UserId = account.Id,
            User = account,
            EnterpriseMembershipId = membershipId,
            TokenHash = hash,
            FamilyId = family,
            SecurityVersionAtIssue = account.SecurityVersion,
            ExpiresAt = expiresAt,
            FamilyExpiresAt = expiresAt
        };
    }

    public bool IsUsable(DateTime now, long version) =>
        ConsumedAt is null && RevokedAt is null && ExpiresAt > now && FamilyExpiresAt > now && SecurityVersionAtIssue == version;

    public void Consume(DateTime now) => ConsumedAt = now;

    public void SetReplacement(long id) => ReplacedByTokenId = id;

    public void Revoke(DateTime now) => RevokedAt ??= now;
    #endregion
}