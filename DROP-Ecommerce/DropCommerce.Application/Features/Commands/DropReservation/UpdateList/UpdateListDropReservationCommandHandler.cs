using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropReservationCommandHandler : IRequestHandler<UpdateListDropReservationCommand, Result<List<DropReservation>>>
{
    public Task<Result<List<DropReservation>>> Handle(UpdateListDropReservationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
