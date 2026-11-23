using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProductReactivationSelector(
    CatalogWriteContext context,
    ProductContractStateReader states)
{
    public async Task<ProductReactivationSelection> SelectAsync(ProductReactivationRequest request, CancellationToken ct)
    {
        if (request is null || request.ExpectedContractRevision <= 0 || request.Mode is not ("oldest" or "selected") ||
            request.ProductIds is { Count: > 5000 } ||
            request.Mode == "oldest" && request.ProductIds is { Count: > 0 } ||
            request.Mode == "selected" && (request.ProductIds is null || request.ProductIds.Count == 0 ||
                request.ProductIds.Any(Id => Id <= 0) || request.ProductIds.Distinct().Count() != request.ProductIds.Count))
            throw new SecurityFailure(400, "ProductContract.InvalidSelection", "Seleção de reativação inválida.");

        var state = await states.ReadAsync(request.ExpectedContractRevision, ct);
        var rights = EnterpriseEntitlements.Resolve(state.PlanId, state.AddOnId);

        if (!rights.AllowsStore)
            throw new SecurityFailure(409, "ProductContract.StoreUnavailable", "O plano não inclui o catálogo da Loja.");

        var products = await context.Products.ListAsync(Product =>
            Product.EnterpriseId == context.Tenant && !Product.IsDeleted, ct);
        var published = products.Count(Product => Product.IsActive && !Product.IsContractBlocked);
        var available = Math.Max(0, rights.StoreProductLimit - published);
        var eligible = products.Where(Product => Product.IsActive && Product.IsContractBlocked)
            .OrderBy(Product => Product.CreatedAt).ThenBy(Product => Product.Id).ToList();
        List<Product> selected;

        if (request.Mode == "oldest")
        {
            selected = eligible.Take(available).ToList();
        }
        else
        {
            selected = eligible.Where(Product => request.ProductIds!.Contains(Product.Id)).ToList();

            if (selected.Count != request.ProductIds!.Count)
                throw new SecurityFailure(404, "ProductContract.SelectionUnavailable",
                    "A seleção contém produto excluído, arquivado, já reativado ou de outra loja.");

            if (selected.Count > available)
                throw new SecurityFailure(409, "ProductContract.PublicationLimit", "Não há capacidade para reativar toda a seleção.");
        }

        if (eligible.Count > 0 && available == 0)
            throw new SecurityFailure(409, "ProductContract.PublicationLimit", "Não há capacidade para reativar produtos.");

        return new ProductReactivationSelection(state, rights, published, eligible.Count, selected);
    }
}
