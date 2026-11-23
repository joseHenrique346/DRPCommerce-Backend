using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RemoveCartItemsCommandHandler(
    ICartRepository carts,
    IProductRepository products,
    ICustomerRepository customers,
    CartWriteScope writes,
    IUnitOfWork unitOfWork,
    CartPurchaseContextReader contexts,
    CartResponseBuilder responses)
    : CartRequestHandler<RemoveCartItemsCommand>
{
    protected override async Task<CartResponse> ExecuteAsync(RemoveCartItemsCommand command, CancellationToken ct)
    {
        var itemIds = command.ItemIds;
        var context = await contexts.ReadAsync(ct);

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(context.EnterpriseId, ct);
        await customers.LockAsync(context.CustomerId, ct);

        if (!await customers.ExistsAsync(Customer => Customer.Id == context.CustomerId && Customer.EnterpriseId == context.EnterpriseId &&
            Customer.IsActive && !Customer.IsDeleted && Customer.PersonalDataErasedAt == null, ct))
            throw new SecurityFailure(401, "Identity.InvalidCredentials", "A conta não está disponível para alterar o carrinho.");

        var cart = await carts.GetForCustomerAsync(context.EnterpriseId, context.CustomerId, tracking: true, ct);

        if (cart is null)
        {
            throw new SecurityFailure(404, "CartItem.NotFound", "Item não encontrado no carrinho.");
        }

        using (writes.Begin(context.EnterpriseId, context.CustomerId))
        {
            foreach (var itemId in itemIds.Distinct())
                CartItemSelection.EnsureItem(cart, itemId);

            foreach (var itemId in itemIds.Distinct())
                carts.RemoveItem(cart.RemoveItem(itemId));
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return await responses.BuildAsync(cart, context, null, ct);
    }
}
