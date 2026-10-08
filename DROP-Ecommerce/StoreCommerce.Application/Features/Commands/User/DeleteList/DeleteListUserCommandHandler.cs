using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListUserCommand, User>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListUserCommand request) => request.ids;
}
