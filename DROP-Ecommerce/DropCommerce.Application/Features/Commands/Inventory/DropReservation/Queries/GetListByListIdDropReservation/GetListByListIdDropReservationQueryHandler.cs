using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropReservationQueryHandler(IRepository<DropReservation> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropReservationQuery, DropReservation>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropReservationQuery request) => request.listId;
}
