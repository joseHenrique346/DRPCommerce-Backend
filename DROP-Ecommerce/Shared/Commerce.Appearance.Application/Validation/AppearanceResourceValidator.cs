using System.Text.Json;
using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;

namespace Commerce.Appearance.Application;

public sealed class AppearanceResourceValidator(IAppearanceResources resources, IMediaAssetReader media)
{
    public async Task ValidateAsync(long enterpriseId, InputAppearanceTarget target, string schemaJson, string settingsJson, CancellationToken ct)
    {
        var references = new HashSet<(string Kind, long Id)>();
        Collect(AppearanceSettingsValidator.Parse(schemaJson), AppearanceSettingsValidator.Parse(settingsJson), references);

        foreach (var (kind, id) in references)
        {
            var exists = kind == "asset"
                ? await media.GetAsync(enterpriseId, id, ct) is not null
                : await resources.ExistsAsync(enterpriseId, target, kind, id, ct);

            if (!exists)
                throw new AppearanceFailure(400, "Appearance.ResourceUnavailable", "Uma referência não existe, pertence a outra loja ou ainda não tem integração disponível.");
        }
    }

    private static void Collect(JsonElement schema, JsonElement value, HashSet<(string Kind, long Id)> references)
    {
        if (value.ValueKind == JsonValueKind.Null)
            return;

        if (schema.TryGetProperty("x-resource", out var kind))
        {
            if (!value.TryGetInt64(out var id) || id < 1)
                throw new AppearanceFailure(400, "Appearance.InvalidReference", "Referência inválida.");

            references.Add((kind.GetString()!, id));
        }

        if (value.ValueKind == JsonValueKind.Object && schema.TryGetProperty("properties", out var fields))
        {
            foreach (var property in value.EnumerateObject())
            {
                if (fields.TryGetProperty(property.Name, out var field))
                    Collect(field, property.Value, references);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array && schema.TryGetProperty("items", out var items))
        {
            foreach (var item in value.EnumerateArray())
                Collect(items, item, references);
        }
    }
}
