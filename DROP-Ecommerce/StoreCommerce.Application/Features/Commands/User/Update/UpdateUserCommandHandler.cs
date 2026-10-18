using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateUserCommandHandler(IMediator mediator) : IRequestHandler<UpdateUserCommand, Result<User>>
{
    public async Task<Result<User>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListUserCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<User>.Success(result.Content![0])
            : Result<User>.Failure(result.Errors);
    }
}
