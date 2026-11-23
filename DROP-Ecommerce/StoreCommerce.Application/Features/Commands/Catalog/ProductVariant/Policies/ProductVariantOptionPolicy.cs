using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using System.Text.Json;

namespace StoreCommerce.Application.Features.Commands;

public static class ProductVariantOptionPolicy
{
    public static void ValidateOptionNames(Product product, ProductVariant variant)
    {
        var schema = product.ListProductVariant.FirstOrDefault(Variant => !Variant.IsDefault && !Variant.IsDeleted && Variant.Id != variant.Id);

        if (schema is null)
            return;

        var expected = JsonSerializer.Deserialize<Dictionary<string, string>>(schema.AttributesJson)!.Keys
            .Select(Key => Key.Trim().Normalize().ToUpperInvariant()).OrderBy(Key => Key, StringComparer.Ordinal);
        var actual = JsonSerializer.Deserialize<Dictionary<string, string>>(variant.AttributesJson)!.Keys
            .Select(Key => Key.Trim().Normalize().ToUpperInvariant()).OrderBy(Key => Key, StringComparer.Ordinal);

        if (!expected.SequenceEqual(actual))
            throw new SecurityFailure(400, "ProductVariant.OptionSchema", "Todas as variantes do produto devem usar as mesmas opções, como tamanho e cor.");
    }
}
