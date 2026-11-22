using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class EnableProductVariantsCommandHandler(ProductCatalogWriter writer) : CatalogRequestHandler<EnableProductVariantsCommand, Product>
{
    protected override Task<Product> ExecuteAsync(EnableProductVariantsCommand request, CancellationToken ct)
    {
        return writer.MutateAsync(request.Id, Product => Product.EnableVariants(), ct);
    }
}
