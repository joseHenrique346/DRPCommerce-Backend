using System.Text.Json;

namespace SharedCommerce.Application.Contracts.Appearance;

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
