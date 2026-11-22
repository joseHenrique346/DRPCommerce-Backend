using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropEventCommandHandler(IRepository<DropEvent> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropEventCommand, CreateListDropEventCommand, DropEvent>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropEventCommand> GetCommandList(CreateListDropEventCommand request) => request.commands;

    protected override DropEvent CreateEntity(CreateDropEventCommand command) =>
        DropEvent.Create(command.enterpriseId, command.productId, command.name, command.slug, command.description, command.coverImageUrl, command.bannerImageUrl, command.statusId, command.totalUnitsAvailable, command.unitsReserved, command.unitsSold, command.price, command.requiresRegistration, command.isPublic, command.registrationStartsAt, command.registrationEndsAt, command.queueOpensAt, command.dropStartsAt, command.dropEndsAt);
}
