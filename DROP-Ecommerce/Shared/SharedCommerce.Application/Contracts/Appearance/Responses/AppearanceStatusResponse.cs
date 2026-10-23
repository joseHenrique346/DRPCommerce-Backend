namespace SharedCommerce.Application.Contracts.Appearance;

public sealed record AppearanceStatusResponse(
    string Target,
    long? PublishedRevisionId,
    long Version,
    bool MigrationBlocked,
    string? MigrationFailureCode);
