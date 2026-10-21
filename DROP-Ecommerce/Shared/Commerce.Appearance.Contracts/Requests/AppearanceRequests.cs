using System.Text.Json;

namespace Commerce.Appearance.Contracts;

public sealed record BeginAppearanceEditRequest(string ThemeKey, string ThemeVersion, bool UseDefaults = false);

public sealed record SaveAppearanceRequest(JsonElement Settings);

public sealed record RollbackAppearanceRequest(long? ExpectedRevisionId);
