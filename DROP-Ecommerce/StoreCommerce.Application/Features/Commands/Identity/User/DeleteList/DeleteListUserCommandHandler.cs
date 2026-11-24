using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListUserCommand, User>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListUserCommand request) => request.ids;
}
