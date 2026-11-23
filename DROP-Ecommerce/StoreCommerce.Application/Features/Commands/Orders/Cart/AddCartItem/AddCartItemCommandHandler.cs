using CartEntity = StoreCommerce.Domain.Entity.Cart;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AddCartItemCommandHandler(
    ICartRepository carts,
    IProductRepository products,
    ICustomerRepository customers,
    CartWriteScope writes,
    IUnitOfWork unitOfWork,
    CartPurchaseContextReader contexts,
    CartResponseBuilder responses)
    : CartRequestHandler<AddCartItemCommand>
{
    protected override async Task<CartResponse> ExecuteAsync(AddCartItemCommand command, CancellationToken ct)
    {
        var productId = command.ProductId;
        var productVariantId = command.ProductVariantId;
        var quantity = command.Quantity;
        var context = await contexts.ReadAsync(ct);
        var product = (await products.GetWithVariantsAsync([productId], ct)).SingleOrDefault();

        if (product is null || !product.IsActive || product.IsContractBlocked || !context.VisibleCategories.Contains(product.CategoryId))
            throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado.");

        var variant = productVariantId.HasValue
            ? product.ListProductVariant.SingleOrDefault(variant => variant.Id == productVariantId)
            : product.ListProductVariant.SingleOrDefault(variant => variant.IsDefault && !product.HasVariants);

        if (variant is null)
            throw new SecurityFailure(400, "ProductVariant.Required", product.HasVariants
                ? "Selecione uma variante do produto."
                : "SKU não pertence ao produto.");

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(context.EnterpriseId, ct);
        await customers.LockAsync(context.CustomerId, ct);

        if (!await customers.ExistsAsync(Customer => Customer.Id == context.CustomerId && Customer.EnterpriseId == context.EnterpriseId &&
            Customer.IsActive && !Customer.IsDeleted && Customer.PersonalDataErasedAt == null, ct))
            throw new SecurityFailure(401, "Identity.InvalidCredentials", "A conta não está disponível para alterar o carrinho.");

        var cart = await carts.GetForCustomerAsync(context.EnterpriseId, context.CustomerId, tracking: true, ct);

        if (cart is null)
        {
            cart = CartEntity.Create(context.EnterpriseId, context.CustomerId);
            carts.Add(cart);
        }

        using (writes.Begin(context.EnterpriseId, context.CustomerId))
        {
            cart.AddItem(product, variant, quantity, context.Settings.AllowBackorder);
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return await responses.BuildAsync(cart, context, null, ct);
    }
}
