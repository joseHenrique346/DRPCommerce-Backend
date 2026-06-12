using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropNotificationCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropNotificationCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropNotificationCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropNotificationCommand([request.id]), cancellationToken);
    }
}
