using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEnterpriseDomainCommandHandler(IEnterpriseDomainRepository repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListEnterpriseDomainCommand, EnterpriseDomain>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListEnterpriseDomainCommand request) => request.ids;
}
