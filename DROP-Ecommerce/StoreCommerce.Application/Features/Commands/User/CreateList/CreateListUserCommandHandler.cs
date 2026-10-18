using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateUserCommand, CreateListUserCommand, User>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateUserCommand> GetCommandList(CreateListUserCommand request) => request.commands;

    protected override User CreateEntity(CreateUserCommand command) =>
        User.Create(command.fullName, UserEmail.Create(command.email), command.isActive);
}
