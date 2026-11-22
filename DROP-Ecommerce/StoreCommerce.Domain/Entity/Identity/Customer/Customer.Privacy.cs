namespace StoreCommerce.Domain.Entity;

public partial class Customer
{
    public DateTime? PersonalDataErasedAt { get; private set; }

    public void ErasePersonalData(DateTime now)
    {
        if (PersonalDataErasedAt.HasValue)
            return;

        FullName = "Cliente excluído";
        Email = CustomerEmail.Create($"deleted-{Guid.NewGuid():N}@deleted.invalid");
        NormalizedEmail = Email.Value.ToUpperInvariant();
        Phone = CustomerPhone.Erased();
        PasswordHash = null;
        EmailConfirmed = false;
        AddressLine = string.Empty;
        City = string.Empty;
        StateId = null;
        ZipCode = string.Empty;
        Country = string.Empty;
        Gender = string.Empty;
        DateOfBirth = null;
        IsVerified = false;
        IsActive = false;
        IsDeleted = true;
        DeletedAt = now;
        LastLoginAt = null;
        FailedLoginCount = 0;
        LockoutUntil = null;
        LastFailedLoginAt = null;
        PersonalDataErasedAt = now;
        InvalidateSessions();
    }
}
