using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdShipmentQueryHandler(IRepository<Shipment> repository)
    : BaseGetListByListIdHandler<GetListByListIdShipmentQuery, Shipment>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdShipmentQuery request) => request.listId;
}
