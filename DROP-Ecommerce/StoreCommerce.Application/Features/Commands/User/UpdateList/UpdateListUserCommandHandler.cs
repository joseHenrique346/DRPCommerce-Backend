using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateUserCommand, UpdateListUserCommand, User>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateUserCommand> GetCommandList(UpdateListUserCommand request) => request.commands;

    protected override long GetById(UpdateUserCommand command) => command.id;

    protected override void ApplyChanges(User entity, UpdateUserCommand command) =>
        entity.UpdateDetails(command.fullName, UserEmail.Create(command.email), command.isActive);
}
