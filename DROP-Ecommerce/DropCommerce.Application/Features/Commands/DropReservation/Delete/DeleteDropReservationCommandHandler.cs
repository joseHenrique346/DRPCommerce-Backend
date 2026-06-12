using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropReservationCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropReservationCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropReservationCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropReservationCommand([request.id]), cancellationToken);
    }
}
