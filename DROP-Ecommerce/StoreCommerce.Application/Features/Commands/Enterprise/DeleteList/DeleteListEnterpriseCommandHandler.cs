using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEnterpriseCommandHandler(IRepository<Enterprise> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListEnterpriseCommand, Enterprise>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListEnterpriseCommand request) => request.ids;
}
