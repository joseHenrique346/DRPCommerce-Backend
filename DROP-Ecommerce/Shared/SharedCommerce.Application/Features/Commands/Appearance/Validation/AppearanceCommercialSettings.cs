using System.Text.Json;
using System.Text.Json.Nodes;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.StaticEntity;

namespace SharedCommerce.Application.Validation;

public static class AppearanceCommercialSettings
{
    public static bool Allows(ThemeRelease release, AppearanceCapabilities rights)
    {
        var channel = rights.Channel switch
        {
            "store" => AppearanceChannel.Store.Id,
            "drop" => AppearanceChannel.Drop.Id,
            _ => 0
        };

        return rights.AllowsModule && channel > 0 && release.AppearanceChannelId == channel &&
            release.CatalogPosition > 0 && release.CatalogPosition <= rights.ThemeLimit;
    }

    public static void EnsureAllowed(ThemeRelease release, string settings, AppearanceCapabilities rights)
    {
        if (!Allows(release, rights))
            throw new AppearanceFailure(409, "Appearance.ThemeNotAllowed", "O tema não está disponível no contrato atual.");

        var supplied = AppearanceSettingsValidator.Canonicalize(AppearanceSettingsValidator.Parse(settings));

        if (Project(release, supplied, rights) != supplied)
            throw new AppearanceFailure(409, "Appearance.CustomizationNotAllowed", "Esses campos de personalização não estão disponíveis no contrato atual.");
    }

    public static string Project(ThemeRelease release, string settings, AppearanceCapabilities rights)
    {
        if (rights.AllowsFullCustomization)
            return AppearanceSettingsValidator.Canonicalize(AppearanceSettingsValidator.Parse(settings));

        var schema = AppearanceSettingsValidator.Parse(release.SchemaJson);
        var defaults = AppearanceSettingsValidator.Parse(release.DefaultsJson);
        var supplied = AppearanceSettingsValidator.Parse(settings);
        var result = ProjectNode(schema, defaults, supplied, rights, "", null);

        return AppearanceSettingsValidator.Canonicalize(AppearanceSettingsValidator.Parse(result?.ToJsonString() ?? "{}"));
    }

    private static JsonNode? ProjectNode(JsonElement schema, JsonElement defaults, JsonElement supplied,
        AppearanceCapabilities rights, string path, string? inherited)
    {
        if (schema.ValueKind != JsonValueKind.Object)
            return defaults.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ? null : JsonNode.Parse(defaults.GetRawText());

        var level = schema.TryGetProperty("x-plan", out var annotation) ? annotation.GetString() : inherited;
        var asset = schema.TryGetProperty("x-resource", out var resource) && resource.GetString() == "asset";
        var logo = level == "logo" || path == "brand.logoAssetId";
        var business = level == "business" || path == "brand.name";

        if ((defaults.ValueKind == JsonValueKind.Object || supplied.ValueKind == JsonValueKind.Object) &&
            schema.TryGetProperty("properties", out var properties))
        {
            var result = new JsonObject();

            foreach (var field in properties.EnumerateObject())
            {
                var fallback = defaults.ValueKind == JsonValueKind.Object && defaults.TryGetProperty(field.Name, out var defaultField)
                    ? defaultField : default;
                var value = supplied.ValueKind == JsonValueKind.Object && supplied.TryGetProperty(field.Name, out var suppliedField)
                    ? suppliedField : fallback;
                var childPath = path.Length == 0 ? field.Name : path + "." + field.Name;
                var projected = ProjectNode(field.Value, fallback, value, rights, childPath, level);

                if (projected is not null || fallback.ValueKind != JsonValueKind.Undefined || value.ValueKind != JsonValueKind.Undefined)
                    result[field.Name] = projected;
            }

            return result;
        }

        var allowed = asset ? rights.AllowsLogo && logo : business || rights.AllowsLogo && logo;
        var selected = allowed ? supplied : defaults;

        if (asset && !rights.AllowsLogo)
            return null;

        if (selected.ValueKind == JsonValueKind.Array && schema.TryGetProperty("items", out var items))
        {
            var result = new JsonArray();

            foreach (var item in selected.EnumerateArray())
                result.Add(ProjectNode(items, default, item, rights, path + "[]", allowed ? level : null));

            return result;
        }

        return selected.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ? null : JsonNode.Parse(selected.GetRawText());
    }
}
