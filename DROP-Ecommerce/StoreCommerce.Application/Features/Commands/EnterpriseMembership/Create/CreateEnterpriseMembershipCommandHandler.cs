using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEnterpriseMembershipCommandHandler(IMediator mediator) : IRequestHandler<CreateEnterpriseMembershipCommand, Result<EnterpriseMembership>>
{
    public async Task<Result<EnterpriseMembership>> Handle(CreateEnterpriseMembershipCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListEnterpriseMembershipCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<EnterpriseMembership>.Success(result.Content![0])
            : Result<EnterpriseMembership>.Failure(result.Errors);
    }
}
