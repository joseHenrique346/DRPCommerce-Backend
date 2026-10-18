using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEnterpriseMembershipCommandHandler(IMediator mediator) : IRequestHandler<UpdateEnterpriseMembershipCommand, Result<EnterpriseMembership>>
{
    public async Task<Result<EnterpriseMembership>> Handle(UpdateEnterpriseMembershipCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListEnterpriseMembershipCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<EnterpriseMembership>.Success(result.Content![0])
            : Result<EnterpriseMembership>.Failure(result.Errors);
    }
}
