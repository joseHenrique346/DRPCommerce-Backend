using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllShipmentQueryHandler(IRepository<Shipment> repository)
    : BaseGetAllHandler<GetAllShipmentQuery, Shipment>(repository);
