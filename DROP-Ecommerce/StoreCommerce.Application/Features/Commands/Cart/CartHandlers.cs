using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Converte as recusas do carrinho (regras de domínio e falhas esperadas) em Result com a mensagem para o cliente.
/// </summary>
public abstract class CartRequestHandler<TRequest> : IRequestHandler<TRequest, Result<CartResponse>>
    where TRequest : IRequest<Result<CartResponse>>
{
    public async Task<Result<CartResponse>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<CartResponse>.Success(await ExecuteAsync(request, cancellationToken));
        }
        catch (SecurityFailure failure)
        {
            return Result<CartResponse>.Failure(IdentityError.FromFailure(failure));
        }
        catch (ArgumentException exception)
        {
            return Result<CartResponse>.Validation("Cart.InvalidOperation", exception.Message);
        }
    }

    protected abstract Task<CartResponse> ExecuteAsync(TRequest request, CancellationToken ct);
}

public sealed class GetCartQueryHandler(CartService cart) : CartRequestHandler<GetCartQuery>
{
    protected override Task<CartResponse> ExecuteAsync(GetCartQuery request, CancellationToken ct) => cart.GetAsync(request.CouponCode, ct);
}

public sealed class AddCartItemCommandHandler(CartService cart) : CartRequestHandler<AddCartItemCommand>
{
    protected override Task<CartResponse> ExecuteAsync(AddCartItemCommand request, CancellationToken ct) =>
        cart.AddItemAsync(request.ProductId, request.ProductVariantId, request.Quantity, ct);
}

public sealed class UpdateCartItemQuantityCommandHandler(CartService cart) : CartRequestHandler<UpdateCartItemQuantityCommand>
{
    protected override Task<CartResponse> ExecuteAsync(UpdateCartItemQuantityCommand request, CancellationToken ct) =>
        cart.UpdateQuantityAsync(request.ItemId, request.Quantity, ct);
}

public sealed class RemoveCartItemsCommandHandler(CartService cart) : CartRequestHandler<RemoveCartItemsCommand>
{
    protected override Task<CartResponse> ExecuteAsync(RemoveCartItemsCommand request, CancellationToken ct) =>
        cart.RemoveItemsAsync(request.ItemIds, ct);
}

public sealed class ClearCartCommandHandler(CartService cart) : CartRequestHandler<ClearCartCommand>
{
    protected override Task<CartResponse> ExecuteAsync(ClearCartCommand request, CancellationToken ct) => cart.ClearAsync(ct);
}

public sealed class SetCartSelectionCommandHandler(CartService cart) : CartRequestHandler<SetCartSelectionCommand>
{
    protected override Task<CartResponse> ExecuteAsync(SetCartSelectionCommand request, CancellationToken ct) =>
        cart.SetSelectionAsync(request.ItemIds, request.Selected, ct);
}

public sealed class AwaitCartItemStockCommandHandler(CartService cart) : CartRequestHandler<AwaitCartItemStockCommand>
{
    protected override Task<CartResponse> ExecuteAsync(AwaitCartItemStockCommand request, CancellationToken ct) =>
        cart.AwaitStockAsync(request.ItemId, ct);
}
