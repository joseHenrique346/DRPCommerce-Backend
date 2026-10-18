using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListAccessRoleCommandHandler(IAccessRoleRepository repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListAccessRoleCommand, AccessRole>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListAccessRoleCommand request) => request.ids;
}
