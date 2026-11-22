using StoreCommerce.Application.Contracts;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PreviewProductReactivationQueryHandler(ProductContractService service)
    : CatalogRequestHandler<PreviewProductReactivationQuery, ProductReactivationPreview>
{
    protected override Task<ProductReactivationPreview> ExecuteAsync(PreviewProductReactivationQuery request, CancellationToken ct)
    {
        return service.PreviewReactivationAsync(request.Request, ct);
    }
}
