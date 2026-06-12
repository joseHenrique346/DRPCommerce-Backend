using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropRegistrationCommandHandler(IMediator mediator) : IRequestHandler<CreateDropRegistrationCommand, Result<DropRegistration>>
{
    public async Task<Result<DropRegistration>> Handle(CreateDropRegistrationCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropRegistrationCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropRegistration>.Success(result.Content.First())
            : Result<DropRegistration>.Failure(result.ListMessageErrors.First());
    }
}
