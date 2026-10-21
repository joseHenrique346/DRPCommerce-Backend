using System.Text.Json;

namespace Commerce.Appearance.Contracts;

public sealed record AppearancePreviewResponse(AppearanceEditResponse Session, AppearanceManifest Manifest);

public sealed record AppearanceStatusResponse(
    string Target,
    long? PublishedRevisionId,
    long Version,
    bool MigrationBlocked,
    string? MigrationFailureCode);

public sealed record AppearanceMigrationPreflightResponse(
    bool HasMigration,
    string? SourceVersion,
    string? TargetVersion,
    DateTime? Deadline,
    bool IsValid,
    string? FailureCode);

public sealed record AppearanceEditResponse(
    long SessionId,
    long Version,
    long EditorId,
    string State,
    DateTime ExpiresAt,
    long? BaseRevisionId,
    ThemeIdentity Theme,
    JsonElement Settings);

public sealed record AppearanceRevisionResponse(
    long Id,
    long Number,
    ThemeIdentity Theme,
    string Checksum,
    long PublishedBy,
    DateTime PublishedAt,
    long? RestoredFromRevisionId,
    string Origin);
