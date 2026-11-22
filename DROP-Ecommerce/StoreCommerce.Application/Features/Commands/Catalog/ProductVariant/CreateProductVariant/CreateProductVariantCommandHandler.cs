using StoreCommerce.Application.Contracts.Product;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateProductVariantCommandHandler(ProductVariantWriter writer) : CatalogRequestHandler<CreateProductVariantCommand, ProductVariantResponse>
{
    protected override Task<ProductVariantResponse> ExecuteAsync(CreateProductVariantCommand request, CancellationToken ct)
    {
        return writer.CreateAsync(request, ct);
    }
}
