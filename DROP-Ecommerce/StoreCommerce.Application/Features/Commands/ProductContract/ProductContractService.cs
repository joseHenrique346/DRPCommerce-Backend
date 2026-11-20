using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProductContractService(
    CatalogWriteContext context,
    IEnterpriseRepository enterprises,
    ProductReactivationScope writes,
    ICurrentIdentity actor)
{
    private sealed record Selection(
        EnterpriseContractState State,
        EnterpriseEntitlements Rights,
        int Published,
        int EligibleCount,
        IReadOnlyList<Product> Products);

    public async Task<ProductReactivationPreview> PreviewReactivationAsync(ProductReactivationRequest request, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.ProductsEdit);
        await using var transaction = await context.BeginAsync(ct);
        var selection = await SelectAsync(request, ct);
        await transaction.CommitAsync(ct);

        return new ProductReactivationPreview(context.Tenant, selection.State.Revision, request.Mode,
            selection.Rights.StoreProductLimit, selection.Published,
            Math.Max(0, selection.Rights.StoreProductLimit - selection.Published),
            selection.EligibleCount, selection.Products.Select(Candidate).ToList());
    }

    public async Task<ProductReactivationResult> ReactivateAsync(ProductReactivationRequest request, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.ProductsEdit);
        await using var transaction = await context.BeginAsync(ct);
        var selection = await SelectAsync(request, ct);

        if (selection.Products.Count > 0)
        {
            using var operation = writes.Begin(context.Tenant, selection.State.Revision);

            foreach (var product in selection.Products)
                product.UnblockForContract();

            await context.UnitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return new ProductReactivationResult(context.Tenant, selection.State.Revision,
            selection.Products.Select(Product => Product.Id).ToList(), selection.Published + selection.Products.Count);
    }

    private async Task<Selection> SelectAsync(ProductReactivationRequest request, CancellationToken ct)
    {
        if (request is null || request.ExpectedContractRevision <= 0 || request.Mode is not ("oldest" or "selected") ||
            request.ProductIds is { Count: > 5000 } ||
            request.Mode == "oldest" && request.ProductIds is { Count: > 0 } ||
            request.Mode == "selected" && (request.ProductIds is null || request.ProductIds.Count == 0 ||
                request.ProductIds.Any(Id => Id <= 0) || request.ProductIds.Distinct().Count() != request.ProductIds.Count))
            throw new SecurityFailure(400, "ProductContract.InvalidSelection", "Seleção de reativação inválida.");

        var state = await ReadStateAsync(request.ExpectedContractRevision, ct);
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

        return new Selection(state, rights, published, eligible.Count, selected);
    }

    public async Task<ProductContractImpactResponse> PreviewContractImpactAsync(ProductContractImpactRequest request, CancellationToken ct)
    {
        actor.RequirePrimaryOwner();

        if (request is null || request.CatalogVersion != EnterpriseCommercialCatalog.Version ||
            EnterprisePlan.Find(request.PlanId) is null ||
            request.AddOnId.HasValue && EnterpriseAddOn.Find(request.AddOnId.Value) is null)
            throw new SecurityFailure(400, "ProductContract.InvalidOffer", "Atualize o catálogo e selecione uma oferta válida.");

        if (request.AddOnId.HasValue && !EnterpriseAddOn.FromId(request.AddOnId.Value).IsEligibleFor(request.PlanId))
            throw new SecurityFailure(400, "Contract.IncompatibleAddOn", "Adicional incompatível com o plano.");

        await using var transaction = await context.BeginAsync(ct);
        var state = await ReadStateAsync(request.ExpectedContractRevision, ct);
        var rights = EnterpriseEntitlements.Resolve(request.PlanId, request.AddOnId);
        var products = await context.Products.ListAsync(Product =>
            Product.EnterpriseId == context.Tenant && !Product.IsDeleted, ct, tracking: false);
        var published = products.Count(Product => Product.IsActive && !Product.IsContractBlocked);
        var affected = rights.AllowsStore
            ? ProductContractPolicy.SelectExcess(products, rights.StoreProductLimit)
            : products.Where(Product => !Product.IsContractBlocked).OrderBy(Product => Product.CreatedAt).ThenBy(Product => Product.Id).ToList();
        await transaction.CommitAsync(ct);

        return new ProductContractImpactResponse(
            context.Tenant, state.Revision, request.PlanId, request.AddOnId,
            products.Count, rights.StoreProductLimit, published,
            published - affected.Count(Product => Product.IsActive), affected.Count,
            products.Count(Product => Product.IsContractBlocked),
            Math.Max(0, products.Count - rights.StoreProductLimit), affected.Count > 100,
            affected.Take(100).Select(Candidate).ToList());
    }

    private async Task<EnterpriseContractState> ReadStateAsync(long expectedRevision, CancellationToken ct)
    {
        var state = await enterprises.GetContractStateAsync(context.Tenant, ct)
            ?? throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        if (expectedRevision <= 0 || state.Revision != expectedRevision)
            throw new SecurityFailure(409, "Contract.RevisionChanged", "O contrato mudou. Atualize a consulta.");

        return state;
    }

    private static ProductReactivationCandidate Candidate(Product product)
    {
        return new ProductReactivationCandidate(product.Id, product.Name, product.SKU, product.CreatedAt, product.ContractBlockReason);
    }
}
