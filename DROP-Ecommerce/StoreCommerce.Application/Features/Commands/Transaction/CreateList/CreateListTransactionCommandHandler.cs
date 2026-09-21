using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListTransactionCommandHandler(IRepository<Transaction> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateTransactionCommand, CreateListTransactionCommand, Transaction>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateTransactionCommand> GetCommandList(CreateListTransactionCommand request) => request.commands;

    protected override Transaction CreateEntity(CreateTransactionCommand command) =>
        Transaction.Create(command.orderId, command.customerId, command.typeId, command.methodId, command.statusId, command.amount, command.fee, command.gatewayReference, command.gatewayProvider, command.gatewayPayload, command.paidAt, command.refundedAt);
}
