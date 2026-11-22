using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListShipmentCommandHandler(IRepository<Shipment> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListShipmentCommand, Shipment>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListShipmentCommand request) => request.ids;
}
