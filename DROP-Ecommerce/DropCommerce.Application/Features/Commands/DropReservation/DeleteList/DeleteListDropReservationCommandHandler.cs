using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropReservationCommandHandler(IRepository<DropReservation> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropReservationCommand, DropReservation>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropReservationCommand request) => request.ids;
}
