using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdShipmentQueryHandler(IRepository<Shipment> repository)
    : BaseGetListByListIdHandler<GetListByListIdShipmentQuery, Shipment>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdShipmentQuery request) => request.listId;
}
