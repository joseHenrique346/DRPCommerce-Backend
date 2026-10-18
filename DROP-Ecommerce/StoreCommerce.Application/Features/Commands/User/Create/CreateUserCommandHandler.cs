using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateUserCommandHandler(IMediator mediator) : IRequestHandler<CreateUserCommand, Result<User>>
{
    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListUserCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<User>.Success(result.Content![0])
            : Result<User>.Failure(result.Errors);
    }
}
