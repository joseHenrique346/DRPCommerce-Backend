using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropRegistrationCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropRegistrationCommand, Result<DropRegistration>>
{
    public async Task<Result<DropRegistration>> Handle(UpdateDropRegistrationCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropRegistrationCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropRegistration>.Success(result.Content.First())
            : Result<DropRegistration>.Failure(result.ListMessageErrors.First());
    }
}
