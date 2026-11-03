using System.Text.Json;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProductVariantWriter(CatalogWriteContext context)
{
    public async Task<ProductVariantResponse> CreateAsync(CreateProductVariantCommand command, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(command.ProductId, ct);

        if (!product.HasVariants)
            throw new SecurityFailure(409, "Product.VariantsDisabled", "Habilite as variantes do produto antes de cadastrar combinações.");

        var request = command.Request;
        var variant = ProductVariant.Create(product, request.Details.Name, request.Details.SKU,
            request.Details.BarCode, request.Details.Attributes, isActive: request.Details.IsActive);
        variant.UpdatePricing(request.Pricing?.Price, request.Pricing?.CostPrice);
        variant.UpdateDimensions(request.Dimensions?.Weight, request.Dimensions?.Height, request.Dimensions?.Width, request.Dimensions?.Length);
        ValidateOptionNames(product, variant);
        await context.ValidateVariantAsync(variant, null, ct);
        await context.Variants.AddAsync(variant, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return ProductVariantResponse.FromEntity(variant);
    }

    public async Task<ProductVariantResponse> MutateAsync(long productId, long id, Action<ProductVariant> mutation, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(productId, ct);
        var variant = product.ListProductVariant.SingleOrDefault(Variant => Variant.Id == id)
            ?? throw new SecurityFailure(404, "ProductVariant.NotFound", "Variante não encontrada neste produto.");

        if (variant.IsDefault)
            throw new SecurityFailure(400, "ProductVariant.DefaultProtected", "Os dados do SKU padrão são gerenciados pelo produto.");

        var previousCombination = variant.CombinationKey;
        mutation(variant);

        if (previousCombination != variant.CombinationKey)
            CatalogWriteContext.EnsureEmptyStock([variant]);

        variant.ValidateAgainstProduct();
        ValidateOptionNames(product, variant);
        await context.ValidateVariantAsync(variant, variant.Id, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return ProductVariantResponse.FromEntity(variant);
    }

    public async Task<bool> DeleteAsync(long productId, long id, CancellationToken ct)
    {
        await MutateAsync(productId, id, Variant => Variant.SoftDelete(), ct);

        return true;
    }

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
