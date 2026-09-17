using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListTransactionCommandHandler(IRepository<Transaction> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateTransactionCommand, UpdateListTransactionCommand, Transaction>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateTransactionCommand> GetCommandList(UpdateListTransactionCommand request) => request.commands;

    protected override long GetById(UpdateTransactionCommand command) => command.id;

    protected override void ApplyChanges(Transaction entity, UpdateTransactionCommand command)
    {
        entity.UpdateDetails(command.typeId, command.methodId, command.statusId, command.amount, command.fee, command.gatewayReference, command.gatewayProvider, command.gatewayPayload);
        entity.UpdateStatus(command.statusId);
    }
}
