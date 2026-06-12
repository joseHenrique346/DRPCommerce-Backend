using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropNotificationCommandHandler(IMediator mediator) : IRequestHandler<CreateDropNotificationCommand, Result<DropNotification>>
{
    public async Task<Result<DropNotification>> Handle(CreateDropNotificationCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropNotificationCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropNotification>.Success(result.Content.First())
            : Result<DropNotification>.Failure(result.ListMessageErrors.First());
    }
}
