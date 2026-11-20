using System.Text.Json;
using System.Text.RegularExpressions;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Application.Validation;

public sealed class ThemeDefinitionValidator(AppearanceSettingsValidator settingsValidator)
{
    public void Validate(InputThemeDefinition definition, string channel)
    {
        if (definition.Channel != channel)
            throw new AppearanceFailure(400, "Appearance.InvalidPackageChannel", "A definição pertence a outro módulo.");

        if (definition.CatalogPosition is < 1 or > 10000)
            throw new AppearanceFailure(400, "Appearance.InvalidCatalogPosition", "A posição comercial do tema deve estar entre 1 e 10000.");

        ValidateCommercialAnnotations(definition.Schema);

        if (string.IsNullOrWhiteSpace(definition.Key) ||
            !Regex.IsMatch(definition.Key, "^[a-z][a-z0-9-]{1,79}$", RegexOptions.CultureInvariant) ||
            !IsVersion(definition.Version) || definition.RendererContract != 1 ||
            string.IsNullOrWhiteSpace(definition.Name) || definition.Name.Length > 200 ||
            definition.Schema.ValueKind != JsonValueKind.Object || definition.Defaults.ValueKind != JsonValueKind.Object)
            throw new AppearanceFailure(400, "Appearance.InvalidPackage", "Definição de tema inválida.");

        try
        {
            settingsValidator.Validate(definition.Schema.GetRawText(), definition.Defaults.GetRawText());
        }
        catch (JsonException)
        {
            throw new AppearanceFailure(400, "Appearance.InvalidSchema", "O schema do tema é inválido.");
        }

        if (definition.ReplacementVersion is not null &&
            (!IsVersion(definition.ReplacementVersion) || definition.MigrationDeadline is null))
            throw new AppearanceFailure(400, "Appearance.InvalidMigration", "Informe a versão substituta e o prazo de migração.");

        if (definition.MigrationDeadline.HasValue &&
            (definition.ReplacementVersion is null || definition.MigrationDeadline.Value.Kind != DateTimeKind.Utc))
            throw new AppearanceFailure(400, "Appearance.InvalidMigration", "O prazo precisa estar em UTC e ter uma versão substituta.");

        if (definition.Migration.ValueKind != JsonValueKind.Undefined)
        {
            if (definition.Migration.ValueKind != JsonValueKind.Object)
                throw new AppearanceFailure(400, "Appearance.InvalidMigration", "A migração precisa ser um objeto.");

            AppearanceSettingsValidator.Parse(definition.Migration.GetRawText());
        }
    }

    private static bool IsVersion(string? version)
    {
        return !string.IsNullOrWhiteSpace(version) &&
            Regex.IsMatch(version, @"^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$", RegexOptions.CultureInvariant) &&
            Version.TryParse(version, out _);
    }

    private static void ValidateCommercialAnnotations(JsonElement schema)
    {
        if (schema.ValueKind != JsonValueKind.Object)
            return;

        if (schema.TryGetProperty("x-plan", out var level))
        {
            if (level.ValueKind != JsonValueKind.String || level.GetString() is not ("business" or "logo" or "full"))
                throw new AppearanceFailure(400, "Appearance.InvalidCommercialMetadata", "Use x-plan business, logo ou full.");

            if (level.GetString() == "logo" && (!schema.TryGetProperty("x-resource", out var kind) ||
                kind.ValueKind != JsonValueKind.String || kind.GetString() != "asset"))
                throw new AppearanceFailure(400, "Appearance.InvalidCommercialMetadata", "x-plan logo precisa identificar uma referência de ativo.");
        }

        if (schema.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in properties.EnumerateObject())
                ValidateCommercialAnnotations(property.Value);
        }

        if (schema.TryGetProperty("items", out var items))
            ValidateCommercialAnnotations(items);
    }
}
