using SharedCommerce.Application.Interfaces.Media;
using System.Text.Json;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Mappings;

public static class AppearanceAssetBindings
{
    public static async Task<AppearanceManifest> ResolveAsync(IMediaAssetReader media, AppearanceManifest manifest, long tenant, CancellationToken ct)
    {
        if (!manifest.AssetBindings.TryGetValue("primaryLogo", out var logo) ||
            await media.GetAsync(tenant, logo.GetProperty("assetId").GetInt64(), ct) is not { } asset)
            return manifest;

        var bindings = manifest.AssetBindings.ToDictionary(Binding => Binding.Key, Binding => Binding.Value);
        bindings["primaryLogo"] = JsonSerializer.SerializeToElement(new { assetId = asset.Id, url = asset.Url });

        return manifest with { AssetBindings = bindings };
    }
}
