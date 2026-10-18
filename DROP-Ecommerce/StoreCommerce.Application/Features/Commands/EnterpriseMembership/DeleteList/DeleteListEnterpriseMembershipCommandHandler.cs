using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEnterpriseMembershipCommandHandler(IEnterpriseMembershipRepository repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListEnterpriseMembershipCommand, EnterpriseMembership>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListEnterpriseMembershipCommand request) => request.ids;
}
