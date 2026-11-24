using StoreCommerce.Domain.Interfaces.Tenancy;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public class InvitationAccessRole : ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long EnterpriseInvitationId { get; private set; }
    public long AccessRoleId { get; private set; }

    #region Navigation Properties
    public EnterpriseInvitation EnterpriseInvitation { get; private set; } = null!;
    public AccessRole AccessRole { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    protected InvitationAccessRole() { }
    #endregion

    #region Functions
    public static InvitationAccessRole Create(EnterpriseInvitation invitation, long roleId)
    {
        return new InvitationAccessRole
        {
            EnterpriseId = invitation.EnterpriseId,
            EnterpriseInvitation = invitation,
            AccessRoleId = roleId
        };
    }
    #endregion
}
