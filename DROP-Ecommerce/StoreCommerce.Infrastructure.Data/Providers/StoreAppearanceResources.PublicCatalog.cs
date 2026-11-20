using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Validation;

namespace StoreCommerce.Infrastructure.Data.Providers;

public sealed partial class StoreAppearanceResources
{
    public async Task<string> FilterPublishedSettingsAsync(
        long enterpriseId, string schemaJson, string settingsJson, CancellationToken ct)
    {
        var schema = AppearanceSettingsValidator.Parse(schemaJson);
        var settings = AppearanceSettingsValidator.Parse(settingsJson);
        var references = new HashSet<long>();
        CollectProductReferences(schema, settings, references);

        if (settings.TryGetProperty("featuredProducts", out var featured) && featured.ValueKind == JsonValueKind.Array)
        {
            foreach (var value in featured.EnumerateArray())
            {
                if (value.TryGetInt64(out var id))
                    references.Add(id);
            }
        }

        if (references.Count == 0)
            return settingsJson;

        var visible = (await db.Products.IgnoreQueryFilters().Where(Product =>
            Product.EnterpriseId == enterpriseId && references.Contains(Product.Id) &&
            Product.IsActive && !Product.IsDeleted && !Product.IsContractBlocked &&
            db.EnterprisePlans.Any(Plan => Plan.Id == Product.Enterprise.PlanId && Plan.AllowsVitrine))
            .Select(Product => Product.Id).ToListAsync(ct)).ToHashSet();
        var filtered = FilterProductReferences(schema, settings, visible);

        if (filtered is JsonObject root && root["featuredProducts"] is JsonArray products)
        {
            for (var index = products.Count - 1; index >= 0; index--)
            {
                if (products[index] is not JsonValue value || !value.TryGetValue<long>(out var id) || !visible.Contains(id))
                    products.RemoveAt(index);
            }
        }

        return AppearanceSettingsValidator.Canonicalize(
            AppearanceSettingsValidator.Parse(filtered?.ToJsonString() ?? "{}"));
    }

    private static bool IsProductReference(JsonElement schema)
    {
        return schema.TryGetProperty("x-resource", out var kind) && kind.GetString() == "product";
    }

    private static void CollectProductReferences(JsonElement schema, JsonElement value, HashSet<long> ids)
    {
        if (IsProductReference(schema) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var id))
        {
            ids.Add(id);

            return;
        }

        if (value.ValueKind == JsonValueKind.Object && schema.TryGetProperty("properties", out var properties))
        {
            foreach (var property in value.EnumerateObject())
            {
                if (properties.TryGetProperty(property.Name, out var childSchema))
                    CollectProductReferences(childSchema, property.Value, ids);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array && schema.TryGetProperty("items", out var items))
        {
            foreach (var item in value.EnumerateArray())
                CollectProductReferences(items, item, ids);
        }
    }

    private static JsonNode? FilterProductReferences(JsonElement schema, JsonElement value, IReadOnlySet<long> visible)
    {
        if (IsProductReference(schema) && value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt64(out var id) && !visible.Contains(id))
            return null;

        if (value.ValueKind == JsonValueKind.Object && schema.TryGetProperty("properties", out var properties))
        {
            var result = new JsonObject();

            foreach (var property in value.EnumerateObject())
                result[property.Name] = properties.TryGetProperty(property.Name, out var childSchema)
                    ? FilterProductReferences(childSchema, property.Value, visible)
                    : JsonNode.Parse(property.Value.GetRawText());

            return result;
        }

        if (value.ValueKind == JsonValueKind.Array && schema.TryGetProperty("items", out var items))
        {
            var result = new JsonArray();

            foreach (var item in value.EnumerateArray())
            {
                var filtered = FilterProductReferences(items, item, visible);

                if (filtered is not null || !IsProductReference(items))
                    result.Add(filtered);
            }

            return result;
        }

        return JsonNode.Parse(value.GetRawText());
    }
}
