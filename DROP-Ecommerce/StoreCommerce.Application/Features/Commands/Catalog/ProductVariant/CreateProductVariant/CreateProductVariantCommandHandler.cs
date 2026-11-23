using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateProductVariantCommandHandler(
    CatalogWriteContext context)
    : CatalogRequestHandler<CreateProductVariantCommand, ProductVariantResponse>
{
    protected override async Task<ProductVariantResponse> ExecuteAsync(CreateProductVariantCommand request, CancellationToken ct)
    {
        await using var transaction = await context.BeginAsync(ct);
        var product = await context.GetProductAsync(request.ProductId, ct);

        if (!product.HasVariants)
            throw new SecurityFailure(409, "Product.VariantsDisabled", "Habilite as variantes do produto antes de cadastrar combinações.");

        var details = request.Request;
        var variant = ProductVariant.Create(product, details.Details.Name, details.Details.SKU,
            details.Details.BarCode, details.Details.Attributes, isActive: details.Details.IsActive);
        variant.UpdatePricing(details.Pricing?.Price, details.Pricing?.CostPrice);
        variant.UpdateDimensions(details.Dimensions?.Weight, details.Dimensions?.Height, details.Dimensions?.Width, details.Dimensions?.Length);
        ProductVariantOptionPolicy.ValidateOptionNames(product, variant);
        await context.ValidateVariantAsync(variant, null, ct);
        await context.Variants.AddAsync(variant, ct);
        await context.UnitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return ProductVariantResponse.FromEntity(variant);
    }
}
