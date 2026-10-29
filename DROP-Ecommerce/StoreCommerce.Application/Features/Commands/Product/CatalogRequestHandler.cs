using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public abstract class CatalogRequestHandler<TRequest, TContent> : IRequestHandler<TRequest, Result<TContent>>
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
    }

    protected abstract Task<TContent> ExecuteAsync(TRequest request, CancellationToken ct);
}
