namespace StoreCommerce.Domain.Entity;

public partial class User
{
    public DateTime? PersonalDataErasedAt { get; private set; }

    public void ErasePersonalData(DateTime now)
    {
        if (PersonalDataErasedAt.HasValue)
            return;

        FullName = "Usuário excluído";
        Email = UserEmail.Create($"deleted-{Guid.NewGuid():N}@deleted.invalid");
        NormalizedEmail = Email.Value.ToUpperInvariant();
        PasswordHash = null;
        EmailConfirmed = false;
        IsActive = false;
        LastLoginAt = null;
        FailedLoginCount = 0;
        LockoutUntil = null;
        LastFailedLoginAt = null;
        PersonalDataErasedAt = now;
        InvalidateSessions();
    }
}
