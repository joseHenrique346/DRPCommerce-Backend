using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropEventCommandHandler(IRepository<DropEvent> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropEventCommand, UpdateListDropEventCommand, DropEvent>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropEventCommand> GetCommandList(UpdateListDropEventCommand request) => request.commands;

    protected override long GetById(UpdateDropEventCommand command) => command.id;

    protected override void ApplyChanges(DropEvent entity, UpdateDropEventCommand command)
    {
        entity.Update(command.enterpriseId, command.productId, command.name, command.slug, command.description, command.coverImageUrl, command.bannerImageUrl, command.statusId, command.totalUnitsAvailable, command.unitsReserved, command.unitsSold, command.price, command.requiresRegistration, command.isPublic, command.registrationStartsAt, command.registrationEndsAt, command.queueOpensAt, command.dropStartsAt, command.dropEndsAt);
    }
}
