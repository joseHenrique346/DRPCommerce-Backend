using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public static class ProductReactivationMapping
{
    public static ProductReactivationCandidate Candidate(Product product)
    {
        return new ProductReactivationCandidate(product.Id, product.Name, product.SKU, product.CreatedAt, product.ContractBlockReason);
    }
}
