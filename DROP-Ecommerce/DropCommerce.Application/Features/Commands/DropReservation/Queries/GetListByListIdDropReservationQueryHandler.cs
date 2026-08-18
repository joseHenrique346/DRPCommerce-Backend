using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropReservationQueryHandler(IRepository<DropReservation> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropReservationQuery, DropReservation>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropReservationQuery request) => request.listId;
}
