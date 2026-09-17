using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdShipmentQueryHandler(IRepository<Shipment> repository)
    : BaseGetByIdHandler<GetByIdShipmentQuery, Shipment>(repository)
{
    protected override long GetById(GetByIdShipmentQuery request) => request.id;
}
