using System.Text.Json;

namespace Commerce.Appearance.Contracts;

public sealed record OutputThemeDescription(
    long Id,
    string Key,
    string Version,
    string Name,
    int RendererContract,
    bool IsDeprecated,
    DateTime? MigrationDeadline,
    JsonElement Schema,
    JsonElement Defaults);
