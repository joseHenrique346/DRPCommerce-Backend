using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropRegistrationCommandHandler(IRepository<DropRegistration> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropRegistrationCommand, DropRegistration>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropRegistrationCommand request) => request.ids;
}
