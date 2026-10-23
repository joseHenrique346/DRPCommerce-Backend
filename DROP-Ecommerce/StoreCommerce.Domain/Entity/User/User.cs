using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public partial class User : BaseEntity
{
    #region Properties
    public string FullName { get; private set; } = string.Empty;
    public UserEmail Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore]
    public string? PasswordHash { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public bool IsActive { get; private set; }
    public long SecurityVersion { get; private set; } = 1;
    public DateTime? LastLoginAt { get; private set; }

    #region Navigation Properties
    private readonly List<EnterpriseMembership> _listEnterpriseMembership = [];
    public IReadOnlyCollection<EnterpriseMembership> ListEnterpriseMembership => _listEnterpriseMembership.AsReadOnly();
    #endregion

    #endregion

    #region Constructors
    protected User() { }

    private User(string fullName, UserEmail email, bool isActive)
    {
        UpdateDetails(fullName, email, isActive);
    }
    #endregion

    #region Functions
    public static User Create(string fullName, UserEmail email, bool isActive)
    {
        return new User(fullName, email, isActive);
    }

    public void UpdateDetails(string fullName, UserEmail email, bool isActive)
    {
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, 255, nameof(FullName));
        BaseValidate.ValidateNotNull(email, nameof(Email));

        FullName = fullName;
        Email = email;
        NormalizedEmail = email.Value.Trim().ToUpperInvariant();
        IsActive = isActive;
    }
    #endregion
}