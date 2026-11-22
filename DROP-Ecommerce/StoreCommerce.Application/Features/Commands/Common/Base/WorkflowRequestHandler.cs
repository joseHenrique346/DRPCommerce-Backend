using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Orders;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Converte as recusas esperadas dos fluxos de pedido (regras de domínio e falhas de negócio) em Result com a mensagem.
/// </summary>
public abstract class WorkflowRequestHandler<TRequest, TContent> : IRequestHandler<TRequest, Result<TContent>>
    where TRequest : IRequest<Result<TContent>>
{
    public async Task<Result<TContent>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<TContent>.Success(await ExecuteAsync(request, cancellationToken));
        }
        catch (SecurityFailure failure)
        {
            return Result<TContent>.Failure(IdentityError.FromFailure(failure));
        }
        catch (ArgumentException exception)
        {
            return Result<TContent>.Validation("Order.InvalidOperation", exception.Message);
        }
    }

    protected abstract Task<TContent> ExecuteAsync(TRequest request, CancellationToken ct);
}
