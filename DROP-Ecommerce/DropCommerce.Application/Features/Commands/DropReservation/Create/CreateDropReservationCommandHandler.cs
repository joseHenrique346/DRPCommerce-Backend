using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropReservationCommandHandler(IMediator mediator) : IRequestHandler<CreateDropReservationCommand, Result<DropReservation>>
{
    public async Task<Result<DropReservation>> Handle(CreateDropReservationCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropReservationCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropReservation>.Success(result.Content.First())
            : Result<DropReservation>.Failure(result.ListMessageErrors.First());
    }
}
