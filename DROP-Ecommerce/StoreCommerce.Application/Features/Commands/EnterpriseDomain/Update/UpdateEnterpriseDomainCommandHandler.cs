using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEnterpriseDomainCommandHandler(IMediator mediator) : IRequestHandler<UpdateEnterpriseDomainCommand, Result<EnterpriseDomain>>
{
    public async Task<Result<EnterpriseDomain>> Handle(UpdateEnterpriseDomainCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListEnterpriseDomainCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<EnterpriseDomain>.Success(result.Content![0])
            : Result<EnterpriseDomain>.Failure(result.Errors);
    }
}
