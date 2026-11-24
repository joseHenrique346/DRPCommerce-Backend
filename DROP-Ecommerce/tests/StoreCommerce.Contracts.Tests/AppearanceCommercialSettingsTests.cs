using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Validation;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.StaticEntity;
using Xunit;
using System.Text.Json;

namespace StoreCommerce.Contracts.Tests;

public sealed class AppearanceCommercialSettingsTests
{
    [Fact]
    public void Business_annotation_does_not_make_asset_references_available_to_free_plan()
    {
        const string schema = """
            {"type":"object","additionalProperties":false,"properties":{"contact":{"type":"object","x-plan":"business",
            "additionalProperties":false,"properties":{"name":{"type":"string"},"asset":{"type":["integer","null"],"x-resource":"asset"}}}}}
            """;
        const string defaults = """{"contact":{"name":"Loja","asset":null}}""";
        const string supplied = """{"contact":{"name":"Minha loja","asset":123}}""";
        var release = ThemeRelease.Create(AppearanceChannel.Store, "business-test", "1.0.0", "Teste", 1, schema, defaults, "checksum");
        var free = new AppearanceCapabilities(1, "store", 1, true, 1, false, false);
        var failure = Assert.Throws<AppearanceFailure>(() => AppearanceCommercialSettings.EnsureAllowed(release, supplied, free));
        Assert.Equal("Appearance.CustomizationNotAllowed", failure.Code);
    }

    [Theory]
    [InlineData("drop")]
    [InlineData("unknown")]
    public void Release_from_another_channel_never_grants_appearance_access(string channel)
    {
        var release = ThemeRelease.Create(AppearanceChannel.Store, "channel-test", "1.0.0", "Teste", 1,
            """{"type":"object","properties":{},"additionalProperties":false}""", "{}", "checksum");
        var rights = new AppearanceCapabilities(1, channel, 1, true, 20, true, true);
        Assert.False(AppearanceCommercialSettings.Allows(release, rights));
    }

    [Fact]
    public void Invalid_commercial_schema_metadata_is_rejected_before_catalog_registration()
    {
        var definition = new InputThemeDefinition("store", "metadata-test", "1.0.0", "Teste", 1,
            JsonSerializer.Deserialize<JsonElement>("""{"type":"object","additionalProperties":false,"properties":{},"x-plan":123}"""),
            JsonSerializer.Deserialize<JsonElement>("{}"));
        var validator = new ThemeDefinitionValidator(new AppearanceSettingsValidator());
        var failure = Assert.Throws<AppearanceFailure>(() => validator.Validate(definition, "store"));
        Assert.Equal("Appearance.InvalidCommercialMetadata", failure.Code);
    }
}
