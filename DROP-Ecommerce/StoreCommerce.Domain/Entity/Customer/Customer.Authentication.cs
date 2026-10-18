using StoreCommerce.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public partial class Customer : IAuthenticationAccount
{
    [JsonIgnore]
    public int FailedLoginCount
    {
        get; private set;
    }
    [JsonIgnore]
    public DateTime? LockoutUntil
    {
        get; private set;
    }
    [JsonIgnore]
    public DateTime? LastFailedLoginAt
    {
        get; private set;
    }
    public DateTime? LastLoginAt
    {
        get; private set;
    }

    public void SetPassword(string passwordHash, bool invalidateSessions = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;

        if (invalidateSessions)
            InvalidateSessions();

        FailedLoginCount = 0;
        LockoutUntil = null;
        LastFailedLoginAt = null;
    }

    public void ConfirmEmail() => EmailConfirmed = true;

    public void InvalidateSessions() => SecurityVersion = checked(SecurityVersion + 1);

    public void RecordFailedLogin(DateTime now, int threshold, int initialSeconds, int maximumSeconds, int resetMinutes)
    {
        if (LastFailedLoginAt is null || LastFailedLoginAt < now.AddMinutes(-resetMinutes))
            FailedLoginCount = 0;

        FailedLoginCount = Math.Min(FailedLoginCount + 1, threshold + 20);
        LastFailedLoginAt = now;

        if (FailedLoginCount >= threshold)
        {
            var seconds = Math.Min(maximumSeconds, initialSeconds * Math.Pow(2, FailedLoginCount - threshold));
            LockoutUntil = now.AddSeconds(seconds);
        }
    }

    public void RecordSuccessfulLogin(DateTime now)
    {
        FailedLoginCount = 0;
        LockoutUntil = null;
        LastFailedLoginAt = null;
        LastLoginAt = now;
    }
}
