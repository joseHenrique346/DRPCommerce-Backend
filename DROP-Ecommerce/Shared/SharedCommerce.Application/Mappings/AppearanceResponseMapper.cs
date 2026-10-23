using System.Globalization;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Validation;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.StaticEntity;

namespace SharedCommerce.Application.Mappings;

public static class AppearanceResponseMapper
{
    public static string TargetCode(InputAppearanceTarget target)
    {
        return BaseStaticEntity.FromId<AppearanceTargetType>(target.AppearanceTargetTypeId).Code;
    }

    public static string TargetKey(InputAppearanceTarget target)
    {
        var code = TargetCode(target);

        return target.DropEventId.HasValue
            ? $"{code}:{target.DropEventId.Value.ToString(CultureInfo.InvariantCulture)}"
            : code;
    }

    public static AppearanceEditResponse EditResponse(AppearanceEditSession session, ThemeRelease release)
    {
        return new AppearanceEditResponse(session.Id, session.Version, session.EditorId,
            BaseStaticEntity.FromId<AppearanceEditSessionStatus>(session.AppearanceEditSessionStatusId).Code,
            session.ExpiresAt, session.BaseRevisionId, Identity(release), AppearanceSettingsValidator.Parse(session.SettingsJson));
    }

    public static AppearanceManifest Manifest(AppearanceChannel channel, AppearanceRevision? revision, ThemeRelease release, string settings, string source, DateTime date)
    {
        var document = AppearanceSettingsValidator.Parse(settings);
        var assets = new Dictionary<string, System.Text.Json.JsonElement>();
        var bindings = new Dictionary<string, System.Text.Json.JsonElement>();

        if (document.TryGetProperty("brand", out var brand) && brand.TryGetProperty("logoAssetId", out var logo) &&
            logo.ValueKind == System.Text.Json.JsonValueKind.Number)
            assets["primaryLogo"] = System.Text.Json.JsonSerializer.SerializeToElement(new { assetId = logo.GetInt64() });

        if (document.TryGetProperty("featuredProducts", out var products))
            bindings["featuredProducts"] = System.Text.Json.JsonSerializer.SerializeToElement(
                products.EnumerateArray().Select(Product => new { productId = Product.GetInt64() }).ToArray());

        if (document.TryGetProperty("featuredCategory", out var category) && category.ValueKind == System.Text.Json.JsonValueKind.Number)
            bindings["featuredCategory"] = System.Text.Json.JsonSerializer.SerializeToElement(new { categoryId = category.GetInt64() });

        return new AppearanceManifest(1, revision?.Id ?? 0, channel.Code, source, Identity(release),
            document, assets, bindings, revision?.Checksum ?? ConfigurationChecksum(release, settings), date);
    }

    public static string ConfigurationChecksum(ThemeRelease release, string settings)
    {
        return AppearanceSettingsValidator.Checksum(release.Checksum + "\n" +
            AppearanceSettingsValidator.Canonicalize(AppearanceSettingsValidator.Parse(settings)));
    }

    public static ThemeIdentity Identity(ThemeRelease release)
    {
        return new ThemeIdentity(release.Key, release.Version, release.RendererContract);
    }
}
