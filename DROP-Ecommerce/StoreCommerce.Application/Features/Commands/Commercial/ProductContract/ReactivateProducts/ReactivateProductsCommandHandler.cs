using StoreCommerce.Application.Contracts;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReactivateProductsCommandHandler(ProductContractService service)
    : CatalogRequestHandler<ReactivateProductsCommand, ProductReactivationResult>
{
    protected override Task<ProductReactivationResult> ExecuteAsync(ReactivateProductsCommand request, CancellationToken ct)
    {
        return service.ReactivateAsync(request.Request, ct);
    }
}
