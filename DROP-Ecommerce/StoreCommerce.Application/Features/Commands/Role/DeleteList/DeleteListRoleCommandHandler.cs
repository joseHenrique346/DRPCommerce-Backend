using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListRoleCommandHandler(IRepository<Role> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListRoleCommand, Role>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListRoleCommand request) => request.ids;
}
