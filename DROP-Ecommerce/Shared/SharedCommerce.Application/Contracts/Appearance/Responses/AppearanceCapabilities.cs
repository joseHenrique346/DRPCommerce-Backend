namespace SharedCommerce.Application.Contracts.Appearance;

public sealed record AppearanceCapabilities(
    long EnterpriseId, string Channel, long ContractRevision, bool AllowsModule,
    int ThemeLimit, bool AllowsLogo, bool AllowsFullCustomization);
