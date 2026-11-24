using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PreviewProductContractImpactQueryHandler(
    CatalogWriteContext context,
    ICurrentIdentity actor,
    ProductContractStateReader states)
    : CatalogRequestHandler<PreviewProductContractImpactQuery, ProductContractImpactResponse>
{
    protected override async Task<ProductContractImpactResponse> ExecuteAsync(PreviewProductContractImpactQuery command, CancellationToken ct)
    {
        var request = command.Request;
        actor.RequirePrimaryOwner();

        if (request is null || request.CatalogVersion != EnterpriseCommercialCatalog.Version ||
            EnterprisePlan.Find(request.PlanId) is null ||
            request.AddOnId.HasValue && EnterpriseAddOn.Find(request.AddOnId.Value) is null)
            throw new SecurityFailure(400, "ProductContract.InvalidOffer", "Atualize o catálogo e selecione uma oferta válida.");

        if (request.AddOnId.HasValue && !EnterpriseAddOn.FromId(request.AddOnId.Value).IsEligibleFor(request.PlanId))
            throw new SecurityFailure(400, "Contract.IncompatibleAddOn", "Adicional incompatível com o plano.");

        await using var transaction = await context.BeginAsync(ct);
        var state = await states.ReadAsync(request.ExpectedContractRevision, ct);
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
            affected.Take(100).Select(ProductReactivationMapping.Candidate).ToList());
    }
}
