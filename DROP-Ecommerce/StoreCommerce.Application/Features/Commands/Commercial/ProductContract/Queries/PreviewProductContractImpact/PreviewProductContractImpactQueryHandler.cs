using StoreCommerce.Application.Contracts;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PreviewProductContractImpactQueryHandler(ProductContractService service)
    : CatalogRequestHandler<PreviewProductContractImpactQuery, ProductContractImpactResponse>
{
    protected override Task<ProductContractImpactResponse> ExecuteAsync(PreviewProductContractImpactQuery request, CancellationToken ct)
    {
        return service.PreviewContractImpactAsync(request.Request, ct);
    }
}
