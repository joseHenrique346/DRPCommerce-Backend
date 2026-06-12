using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropReservationCommandHandler : IRequestHandler<CreateListDropReservationCommand, Result<List<DropReservation>>>
{
    public Task<Result<List<DropReservation>>> Handle(CreateListDropReservationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
