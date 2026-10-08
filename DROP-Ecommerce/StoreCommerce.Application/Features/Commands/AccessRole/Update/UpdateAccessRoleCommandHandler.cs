using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateAccessRoleCommandHandler(IMediator mediator) : IRequestHandler<UpdateAccessRoleCommand, Result<AccessRole>>
{
    public async Task<Result<AccessRole>> Handle(UpdateAccessRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListAccessRoleCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<AccessRole>.Success(result.Content![0])
            : Result<AccessRole>.Failure(result.Errors);
    }
}
