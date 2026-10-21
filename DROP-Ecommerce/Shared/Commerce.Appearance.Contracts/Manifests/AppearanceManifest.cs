using System.Text.Json;

namespace Commerce.Appearance.Contracts;

public sealed record AppearanceManifest(
    int ManifestVersion,
    long RevisionId,
    string Channel,
    string Source,
    ThemeIdentity Theme,
    JsonElement Settings,
    IReadOnlyDictionary<string, JsonElement> AssetBindings,
    IReadOnlyDictionary<string, JsonElement> ResourceBindings,
    string Checksum,
    DateTime PublishedAt);

public sealed record ThemeIdentity(string Key, string Version, int RendererContract);
