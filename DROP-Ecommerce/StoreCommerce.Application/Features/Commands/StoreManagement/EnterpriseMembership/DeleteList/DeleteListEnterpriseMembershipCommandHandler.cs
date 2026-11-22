using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEnterpriseMembershipCommandHandler(IEnterpriseMembershipRepository repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListEnterpriseMembershipCommand, EnterpriseMembership>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListEnterpriseMembershipCommand request) => request.ids;
}
