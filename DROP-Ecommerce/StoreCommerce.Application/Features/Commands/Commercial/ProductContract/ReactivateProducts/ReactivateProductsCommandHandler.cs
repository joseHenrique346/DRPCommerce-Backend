using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReactivateProductsCommandHandler(
    CatalogWriteContext context,
    ProductReactivationScope writes,
    ICurrentIdentity actor,
    ProductReactivationSelector selectionReader)
    : CatalogRequestHandler<ReactivateProductsCommand, ProductReactivationResult>
{
    protected override async Task<ProductReactivationResult> ExecuteAsync(ReactivateProductsCommand command, CancellationToken ct)
    {
        var request = command.Request;
        actor.RequirePermission(PermissionCodes.ProductsEdit);
        await using var transaction = await context.BeginAsync(ct);
        var selection = await selectionReader.SelectAsync(request, ct);

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
}
