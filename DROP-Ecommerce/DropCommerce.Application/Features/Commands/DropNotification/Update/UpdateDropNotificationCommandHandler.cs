using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropNotificationCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropNotificationCommand, Result<DropNotification>>
{
    public async Task<Result<DropNotification>> Handle(UpdateDropNotificationCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropNotificationCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropNotification>.Success(result.Content.First())
            : Result<DropNotification>.Failure(result.ListMessageErrors.First());
    }
}
