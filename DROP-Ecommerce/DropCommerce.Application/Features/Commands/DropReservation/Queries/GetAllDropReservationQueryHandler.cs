using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropReservationQueryHandler : IRequestHandler<GetAllDropReservationQuery, Result<List<DropReservation>>>
{
    public Task<Result<List<DropReservation>>> Handle(GetAllDropReservationQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
