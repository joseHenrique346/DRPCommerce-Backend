using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Inventory;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetVariantStockQueryHandler(IInventoryRepository repository, ITenantProvider tenant)
    : CatalogRequestHandler<GetVariantStockQuery, ProductVariantResponse>
{
    protected override async Task<ProductVariantResponse> ExecuteAsync(GetVariantStockQuery request, CancellationToken ct)
    {
        var variant = (await repository.GetVariantsAsync(tenant.GetEnterpriseId(), [request.Id], ct)).SingleOrDefault()
            ?? throw new SecurityFailure(404, "ProductVariant.NotFound", "SKU não encontrado na loja atual.");

        return ProductVariantResponse.FromEntity(variant);
    }
}
