namespace SharedCommerce.Application.Contracts.Appearance;

public sealed record AppearanceMigrationPreflightResponse(
    bool HasMigration,
    string? SourceVersion,
    string? TargetVersion,
    DateTime? Deadline,
    bool IsValid,
    string? FailureCode);
