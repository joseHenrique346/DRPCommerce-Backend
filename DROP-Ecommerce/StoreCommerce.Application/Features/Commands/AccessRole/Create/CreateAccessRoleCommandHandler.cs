using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateAccessRoleCommandHandler(IMediator mediator) : IRequestHandler<CreateAccessRoleCommand, Result<AccessRole>>
{
    public async Task<Result<AccessRole>> Handle(CreateAccessRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListAccessRoleCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<AccessRole>.Success(result.Content![0])
            : Result<AccessRole>.Failure(result.Errors);
    }
}
