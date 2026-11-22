using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllShipmentQueryHandler(IRepository<Shipment> repository)
    : BaseGetAllHandler<GetAllShipmentQuery, Shipment>(repository);
