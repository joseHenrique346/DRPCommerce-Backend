using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEnterpriseDomainCommandHandler(IMediator mediator) : IRequestHandler<CreateEnterpriseDomainCommand, Result<EnterpriseDomain>>
{
    public async Task<Result<EnterpriseDomain>> Handle(CreateEnterpriseDomainCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListEnterpriseDomainCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<EnterpriseDomain>.Success(result.Content![0])
            : Result<EnterpriseDomain>.Failure(result.Errors);
    }
}
