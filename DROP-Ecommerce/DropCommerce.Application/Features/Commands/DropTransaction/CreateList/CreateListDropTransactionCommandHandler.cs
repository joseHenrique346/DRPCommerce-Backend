using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropTransactionCommandHandler(IRepository<DropTransaction> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropTransactionCommand, CreateListDropTransactionCommand, DropTransaction>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropTransactionCommand> GetCommandList(CreateListDropTransactionCommand request) => request.commands;

    protected override DropTransaction CreateEntity(CreateDropTransactionCommand command) =>
        DropTransaction.Create(command.dropOrderId, command.customerId, command.typeId, command.methodId, command.statusId, command.amount, command.fee, command.gatewayReference, command.gatewayProvider, command.gatewayPayload, command.paidAt, command.refundedAt);
}
