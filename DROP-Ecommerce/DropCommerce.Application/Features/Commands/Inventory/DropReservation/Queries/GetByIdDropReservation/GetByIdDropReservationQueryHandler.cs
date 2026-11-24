using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropReservationQueryHandler(IRepository<DropReservation> repository)
    : BaseGetByIdHandler<GetByIdDropReservationQuery, DropReservation>(repository)
{
    protected override long GetById(GetByIdDropReservationQuery request) => request.id;
}
