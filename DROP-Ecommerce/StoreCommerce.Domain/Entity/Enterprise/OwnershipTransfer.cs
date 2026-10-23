using StoreCommerce.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public class OwnershipTransfer : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long IssuedByMembershipId { get; private set; }
    public long RecipientMembershipId { get; private set; }
    [JsonIgnore] public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; } = null!;
    public EnterpriseMembership IssuedByMembership { get; private set; } = null!;
    public EnterpriseMembership RecipientMembership { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    protected OwnershipTransfer() { }
    #endregion

    #region Functions
    public static OwnershipTransfer Create(long tenant, long issuer, long recipient, string hash, DateTime expires)
    {
        return new OwnershipTransfer
        {
            EnterpriseId = tenant,
            IssuedByMembershipId = issuer,
            RecipientMembershipId = recipient,
            TokenHash = hash,
            ExpiresAt = expires
        };
    }

    public bool IsUsable(DateTime now) => AcceptedAt is null && RevokedAt is null && ExpiresAt > now;

    public void Accept(DateTime now) => AcceptedAt = now;

    public void Revoke(DateTime now) => RevokedAt ??= now;
    #endregion
}