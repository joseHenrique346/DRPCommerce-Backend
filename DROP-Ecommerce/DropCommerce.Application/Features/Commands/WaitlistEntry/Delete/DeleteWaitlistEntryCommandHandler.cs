using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteWaitlistEntryCommandHandler(IMediator mediator) : IRequestHandler<DeleteWaitlistEntryCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteWaitlistEntryCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListWaitlistEntryCommand([request.id]), cancellationToken);
    }
}
