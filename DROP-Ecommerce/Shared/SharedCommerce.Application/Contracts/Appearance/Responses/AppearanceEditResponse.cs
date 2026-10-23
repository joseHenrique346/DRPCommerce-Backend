using System.Text.Json;

namespace SharedCommerce.Application.Contracts.Appearance;

public sealed record AppearanceEditResponse(
    long SessionId,
    long Version,
    long EditorId,
    string State,
    DateTime ExpiresAt,
    long? BaseRevisionId,
    ThemeIdentity Theme,
    JsonElement Settings);
