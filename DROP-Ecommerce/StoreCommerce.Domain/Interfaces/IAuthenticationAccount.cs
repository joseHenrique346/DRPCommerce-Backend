namespace StoreCommerce.Domain.Interfaces;

public interface IAuthenticationAccount
{
    long Id { get; }
    string? PasswordHash { get; }
    bool EmailConfirmed { get; }
    bool IsActive { get; }
    long SecurityVersion { get; }
    int FailedLoginCount { get; }
    DateTime? LockoutUntil { get; }
    void SetPassword(string passwordHash, bool invalidateSessions = true);
    void ConfirmEmail();
    void InvalidateSessions();
    void RecordFailedLogin(DateTime now, int threshold, int initialSeconds, int maximumSeconds, int resetMinutes);
    void RecordSuccessfulLogin(DateTime now);
}
