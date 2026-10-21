using System.Text.Json;

namespace Commerce.Appearance.Contracts;

public sealed record InputThemeDefinition(
    string Channel,
    string Key,
    string Version,
    string Name,
    int RendererContract,
    JsonElement Schema,
    JsonElement Defaults,
    string? ReplacementVersion = null,
    DateTime? MigrationDeadline = null,
    JsonElement Migration = default);
