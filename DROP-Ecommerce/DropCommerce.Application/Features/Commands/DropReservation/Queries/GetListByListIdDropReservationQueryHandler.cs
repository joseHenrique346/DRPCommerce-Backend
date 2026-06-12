using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropReservationQueryHandler : IRequestHandler<GetListByListIdDropReservationQuery, Result<List<DropReservation>>>
{
    public Task<Result<List<DropReservation>>> Handle(GetListByListIdDropReservationQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
