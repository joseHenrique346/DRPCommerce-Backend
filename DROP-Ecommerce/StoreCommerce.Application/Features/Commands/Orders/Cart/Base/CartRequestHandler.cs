using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;

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
