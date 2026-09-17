using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListServiceCommandHandler(IRepository<Service> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListServiceCommand, Service>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListServiceCommand request) => request.ids;
}
