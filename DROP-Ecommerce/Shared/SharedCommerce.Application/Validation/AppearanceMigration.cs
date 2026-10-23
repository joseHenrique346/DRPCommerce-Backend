using System.Text.Json.Nodes;
using System.Text.Json;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Application.Validation;

public sealed class AppearanceMigration
{
    public string Apply(string previousJson, string defaultsJson, string migrationJson)
    {
        try
        {
            return ApplyCore(previousJson, defaultsJson, migrationJson);
        }
        catch (JsonException)
        {
            throw Invalid();
        }
        catch (InvalidOperationException)
        {
            throw Invalid();
        }
    }

    private static string ApplyCore(string previousJson, string defaultsJson, string migrationJson)
    {
        var previous = JsonNode.Parse(previousJson) as JsonObject ?? throw Invalid();
        var defaults = JsonNode.Parse(defaultsJson) as JsonObject ?? throw Invalid();
        var migration = JsonNode.Parse(migrationJson) as JsonObject ?? throw Invalid();

        if (migration.Any(Property => Property.Key is not ("rename" or "remove")))
            throw Invalid();

        if (migration.TryGetPropertyValue("rename", out var renameValue) && renameValue is not JsonObject ||
            migration.TryGetPropertyValue("remove", out var removeValue) && removeValue is not JsonArray)
            throw Invalid();

        if (migration["rename"] is JsonObject renames)
        {
            foreach (var rename in renames)
            {
                var (parent, key) = Locate(previous, rename.Key);

                if (!parent.TryGetPropertyValue(key, out var value))
                    continue;

                var destination = rename.Value?.GetValue<string>() ?? throw Invalid();
                var (target, targetKey) = Locate(previous, destination);

                if (target.ContainsKey(targetKey))
                    throw Invalid();

                parent.Remove(key);
                target[targetKey] = value;
            }
        }

        if (migration["remove"] is JsonArray removals)
        {
            foreach (var removal in removals)
            {
                var (parent, key) = Locate(previous, removal?.GetValue<string>() ?? throw Invalid());
                parent.Remove(key);
            }
        }

        Merge(defaults, previous);

        return AppearanceSettingsValidator.Canonicalize(AppearanceSettingsValidator.Parse(defaults.ToJsonString()));
    }

    private static (JsonObject Parent, string Key) Locate(JsonObject root, string path)
    {
        var segments = path.Split('.');

        if (segments.Length > 8 || segments.Any(string.IsNullOrWhiteSpace))
            throw Invalid();

        var parent = root;

        foreach (var segment in segments.SkipLast(1))
        {
            if (parent[segment] is not JsonObject child)
                throw Invalid();

            parent = child;
        }

        return (parent, segments[^1]);
    }

    private static void Merge(JsonObject defaults, JsonObject previous)
    {
        foreach (var property in previous)
        {
            if (defaults[property.Key] is JsonObject target && property.Value is JsonObject source)
                Merge(target, source);
            else
                defaults[property.Key] = property.Value?.DeepClone();
        }
    }

    private static AppearanceFailure Invalid()
    {
        return new AppearanceFailure(400, "Appearance.InvalidMigration", "A migração do tema é inválida.");
    }
}
