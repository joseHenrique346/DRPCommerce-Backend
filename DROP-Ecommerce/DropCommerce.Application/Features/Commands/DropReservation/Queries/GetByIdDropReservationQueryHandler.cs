using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropReservationQueryHandler(IRepository<DropReservation> repository)
    : BaseGetByIdHandler<GetByIdDropReservationQuery, DropReservation>(repository)
{
    protected override long GetById(GetByIdDropReservationQuery request) => request.id;
}
