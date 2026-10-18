using StoreCommerce.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public class EnterpriseInvitation : BaseEntity, ITenantEntity
{
    public long EnterpriseId
    {
        get; private set;
    }
    public string InvitedNormalizedEmail { get; private set; } = string.Empty;
    public long InvitedKindId
    {
        get; private set;
    }
    public long IssuedByMembershipId
    {
        get; private set;
    }
    [JsonIgnore]
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt
    {
        get; private set;
    }
    public DateTime? AcceptedAt
    {
        get; private set;
    }
    public DateTime? RevokedAt
    {
        get; private set;
    }
    public Enterprise Enterprise { get; private set; } = null!;
    public EnterpriseMembership IssuedByMembership { get; private set; } = null!;
    public ICollection<InvitationAccessRole> AccessRoles { get; private set; } = [];

    protected EnterpriseInvitation()
    {
    }

    public static EnterpriseInvitation Create(long enterpriseId, string email, long kindId, long issuerId, string hash, DateTime expiresAt)
    {
        if (kindId is not (2 or 3))
            throw new ArgumentException("Convites só admitem sócio ou funcionário.");

        return new()
        {
            EnterpriseId = enterpriseId,
            InvitedNormalizedEmail = email.Trim().ToUpperInvariant(),
            InvitedKindId = kindId,
            IssuedByMembershipId = issuerId,
            TokenHash = hash,
            ExpiresAt = expiresAt
        };
    }

    public bool IsUsable(DateTime now) => AcceptedAt is null && RevokedAt is null && ExpiresAt > now;

    public void Accept(DateTime now) => AcceptedAt = now;

    public void Revoke(DateTime now) => RevokedAt ??= now;
}

public class InvitationAccessRole : ITenantEntity
{
    public long EnterpriseId
    {
        get; private set;
    }
    public long EnterpriseInvitationId
    {
        get; private set;
    }
    public long AccessRoleId
    {
        get; private set;
    }
    public EnterpriseInvitation EnterpriseInvitation { get; private set; } = null!;
    public AccessRole AccessRole { get; private set; } = null!;

    protected InvitationAccessRole()
    {
    }

    public static InvitationAccessRole Create(EnterpriseInvitation invitation, long roleId) =>
        new()
        {
            EnterpriseId = invitation.EnterpriseId,
            EnterpriseInvitation = invitation,
            AccessRoleId = roleId
        };
}
