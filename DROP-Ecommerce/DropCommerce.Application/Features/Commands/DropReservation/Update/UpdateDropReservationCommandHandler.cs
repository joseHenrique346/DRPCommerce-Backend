using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropReservationCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropReservationCommand, Result<DropReservation>>
{
    public async Task<Result<DropReservation>> Handle(UpdateDropReservationCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropReservationCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropReservation>.Success(result.Content.First())
            : Result<DropReservation>.Failure(result.ListMessageErrors.First());
    }
}
