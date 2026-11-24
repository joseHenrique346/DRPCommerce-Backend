using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
using Json.Schema;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Application.Validation;

public sealed class AppearanceSettingsValidator
{
    public string Validate(string schemaJson, string settingsJson)
    {
        var settings = Parse(settingsJson);
        var schemaDocument = Parse(schemaJson);
        EnsureClosedSchema(schemaDocument);

        var schema = JsonSchema.FromText(schemaJson);

        if (!schema.Evaluate(JsonNode.Parse(settings.GetRawText())).IsValid)
            throw new AppearanceFailure(400, "Appearance.InvalidSettings", "As configurações não correspondem aos campos permitidos pelo tema.");

        return Canonicalize(settings);
    }

    public static JsonElement Parse(string json)
    {
        if (Encoding.UTF8.GetByteCount(json) > 65536)
            throw new AppearanceFailure(400, "Appearance.DocumentTooLarge", "A configuração excede 64 KiB.");

        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 24 });
            EnsureUniqueProperties(document.RootElement);

            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new AppearanceFailure(400, "Appearance.InvalidJson", "Documento JSON inválido.");
        }
    }

    public static string Canonicalize(JsonElement value)
    {
        return Sort(value)?.ToJsonString() ?? "null";
    }

    public static string Checksum(string value)
    {
        return "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private static JsonNode? Sort(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var result = new JsonObject();

            foreach (var property in value.EnumerateObject().OrderBy(Property => Property.Name, StringComparer.Ordinal))
                result[property.Name] = Sort(property.Value);

            return result;
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            var result = new JsonArray();

            foreach (var item in value.EnumerateArray())
                result.Add(Sort(item));

            return result;
        }

        return JsonNode.Parse(value.GetRawText());
    }

    private static void EnsureUniqueProperties(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);

            foreach (var property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new AppearanceFailure(400, "Appearance.DuplicateField", "O documento contém campos duplicados.");

                EnsureUniqueProperties(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
                EnsureUniqueProperties(item);
        }
    }

    private static void EnsureClosedSchema(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
            return;

        if (value.TryGetProperty("$ref", out _))
            throw new AppearanceFailure(400, "Appearance.UnsupportedSchema", "Os schemas locais não podem carregar referências externas ou indiretas.");

        if (value.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String && type.GetString() == "object" &&
            (!value.TryGetProperty("additionalProperties", out var additional) || additional.ValueKind != JsonValueKind.False))
            throw new AppearanceFailure(400, "Appearance.OpenSchema", "Objetos do tema precisam declarar additionalProperties: false.");

        foreach (var property in value.EnumerateObject())
        {
            if (property.Name == "properties")
            {
                foreach (var field in property.Value.EnumerateObject())
                    EnsureClosedSchema(field.Value);
            }
            else if (property.Name == "items")
            {
                EnsureClosedSchema(property.Value);
            }
        }
    }
}
