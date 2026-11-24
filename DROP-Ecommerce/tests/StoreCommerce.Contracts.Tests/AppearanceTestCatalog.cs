using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Options;

namespace StoreCommerce.Contracts.Tests;

internal static class AppearanceTestCatalog
{
    public const string Schema = """
        {"type":"object","additionalProperties":false,"required":["brand","accent","font","featuredProducts"],"properties":{
          "brand":{"type":"object","additionalProperties":false,"required":["name","logoAssetId"],"properties":{
            "name":{"type":"string","maxLength":80,"x-plan":"business"},
            "logoAssetId":{"type":["integer","null"],"minimum":1,"x-resource":"asset","x-plan":"logo"}}},
          "accent":{"type":"string","pattern":"^#[a-fA-F0-9]{6}$"},
          "font":{"type":"string","enum":["system","serif"]},
          "featuredProducts":{"type":"array","items":{"type":"integer","minimum":1,"x-resource":"product"}}}}
        """;

    public static object Settings(string name = "Loja", string accent = "#223344", long? logo = null)
    {
        return new { brand = new { name, logoAssetId = logo }, accent, font = "system", featuredProducts = Array.Empty<long>() };
    }

    public static object Definition(int position, string channel = "store", string version = "1.0.0")
    {
        return new { channel, key = "contract-" + position.ToString("D2"), version, name = "Tema " + position,
            rendererContract = 1, schema = JsonSerializer.Deserialize<JsonElement>(Schema), defaults = Settings(), catalogPosition = position };
    }

    public static async Task RegisterAsync(ContractsApiFixture fixture)
    {
        using var scope = fixture.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SecurityOptions>>().Value;
        using var client = fixture.Client();
        client.DefaultRequestHeaders.Add("X-Service-Key", options.InternalServiceKey);
        using var registered = await client.PostAsJsonAsync("/api/internal/appearance-themes",
            new { definitions = Enumerable.Range(1, 20).Select(Position => Definition(Position))
                .Append(Definition(1, version: "1.0.1")).ToArray() });

        if (registered.StatusCode != HttpStatusCode.NoContent)
            throw new InvalidOperationException("Falha no catálogo de teste: " + await registered.Content.ReadAsStringAsync());
    }
}
