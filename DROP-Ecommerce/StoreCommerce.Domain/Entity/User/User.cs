using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public partial class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public UserEmail Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore] 
    public string? PasswordHash { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public bool IsActive { get; private set; }
    public long SecurityVersion { get; private set; } = 1;
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<EnterpriseMembership> _listEnterpriseMembership = [];
    public IReadOnlyCollection<EnterpriseMembership> ListEnterpriseMembership => _listEnterpriseMembership.AsReadOnly();

    protected User() { }

    private User(string fullName, UserEmail email, bool isActive) =>
        UpdateDetails(fullName, email, isActive);

    public static User Create(string fullName, UserEmail email, bool isActive) =>
        new(fullName, email, isActive);

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
}
