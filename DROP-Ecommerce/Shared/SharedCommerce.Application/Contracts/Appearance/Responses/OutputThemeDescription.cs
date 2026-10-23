using System.Text.Json;

namespace SharedCommerce.Application.Contracts.Appearance;

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
