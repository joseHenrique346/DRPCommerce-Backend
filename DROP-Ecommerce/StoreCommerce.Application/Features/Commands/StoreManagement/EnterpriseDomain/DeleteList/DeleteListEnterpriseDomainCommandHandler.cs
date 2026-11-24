using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEnterpriseDomainCommandHandler(IEnterpriseDomainRepository repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListEnterpriseDomainCommand, EnterpriseDomain>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListEnterpriseDomainCommand request) => request.ids;
}
