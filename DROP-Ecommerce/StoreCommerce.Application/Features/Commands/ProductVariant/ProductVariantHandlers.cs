using StoreCommerce.Application.Contracts.Product;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateProductVariantCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<CreateProductVariantCommand, ProductVariantResponse>
{
    protected override Task<ProductVariantResponse> ExecuteAsync(CreateProductVariantCommand request, CancellationToken ct)
    {
        return writer.CreateAsync(request, ct);
    }
}

public sealed class UpdateProductVariantCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<UpdateProductVariantCommand, ProductVariantResponse>
{
    protected override Task<ProductVariantResponse> ExecuteAsync(UpdateProductVariantCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.ProductId, request.Id, Variant => Variant.UpdateDetails(request.Request.Name,
            request.Request.SKU, request.Request.BarCode, request.Request.Attributes, request.Request.IsActive), ct);
    }
}

public sealed class UpdateProductVariantPricingCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<UpdateProductVariantPricingCommand, ProductVariantResponse>
{
    protected override Task<ProductVariantResponse> ExecuteAsync(UpdateProductVariantPricingCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.ProductId, request.Id, Variant => Variant.UpdatePricing(request.Request.Price, request.Request.CostPrice), ct);
    }
}

public sealed class UpdateProductVariantDimensionsCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<UpdateProductVariantDimensionsCommand, ProductVariantResponse>
{
    protected override Task<ProductVariantResponse> ExecuteAsync(UpdateProductVariantDimensionsCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.ProductId, request.Id, Variant => Variant.UpdateDimensions(request.Request.Weight,
            request.Request.Height, request.Request.Width, request.Request.Length), ct);
    }
}

public sealed class DeleteProductVariantCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<DeleteProductVariantCommand, bool>
{
    protected override Task<bool> ExecuteAsync(DeleteProductVariantCommand request, CancellationToken ct)
    {
        return writer.DeleteAsync(request.ProductId, request.Id, ct);
    }
}

public sealed class GetProductVariantsQueryHandler(CatalogWriteContext context) : CatalogRequestHandler<GetProductVariantsQuery, List<ProductVariantResponse>>
{
    protected override async Task<List<ProductVariantResponse>> ExecuteAsync(GetProductVariantsQuery request, CancellationToken ct)
    {
        var product = await context.GetProductAsync(request.ProductId, ct);

        return product.ListProductVariant.OrderByDescending(Variant => Variant.IsDefault).ThenBy(Variant => Variant.Id)
            .Select(ProductVariantResponse.FromEntity).ToList();
    }
}
