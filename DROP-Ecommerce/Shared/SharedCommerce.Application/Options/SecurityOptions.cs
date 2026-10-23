namespace SharedCommerce.Application.Options;

/// <summary>
/// Reúne chaves, prazos e limites configuráveis para manter a segurança fora das regras de negócio.
/// </summary>
public sealed class SecurityOptions
{
    public const string Section = "Security";
    public string Issuer { get; set; } = "DRPCommerce";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 10;
    public int RefreshFamilyDays { get; set; } = 7;
    public int InvitationHours { get; set; } = 48;
    public int OwnershipTransferMinutes { get; set; } = 30;
    public int ActionTokenMinutes { get; set; } = 30;
    public int FailedLoginThreshold { get; set; } = 5;
    public int InitialLockoutSeconds { get; set; } = 30;
    public int MaximumLockoutSeconds { get; set; } = 900;
    public int FailureResetMinutes { get; set; } = 30;
    public string StorefrontBaseDomain { get; set; } = string.Empty;
    public string BackofficeUrl { get; set; } = string.Empty;
    public string InternalServiceKey { get; set; } = string.Empty;
    public string[] AllowedOrigins { get; set; } = [];
    public string[] TrustedProxies { get; set; } = [];
    public bool RequireConfirmedEmail { get; set; }
    public int AuthRequestsPerMinute { get; set; } = 15;
    public int RefreshRequestsPerMinute { get; set; } = 30;
    public int ApiRequestsPerMinute { get; set; } = 180;
    public int TokenRetentionDays { get; set; } = 7;
}
