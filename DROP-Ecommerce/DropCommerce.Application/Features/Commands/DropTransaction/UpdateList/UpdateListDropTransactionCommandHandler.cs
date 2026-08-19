using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropTransactionCommandHandler(IRepository<DropTransaction> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropTransactionCommand, UpdateListDropTransactionCommand, DropTransaction>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropTransactionCommand> GetCommandList(UpdateListDropTransactionCommand request) => request.commands;

    protected override long GetById(UpdateDropTransactionCommand command) => command.id;

    protected override void ApplyChanges(DropTransaction entity, UpdateDropTransactionCommand command)
    {
        entity.Update(command.dropOrderId, command.customerId, command.typeId, command.methodId, command.statusId, command.amount, command.fee, command.gatewayReference, command.gatewayProvider, command.gatewayPayload, command.paidAt, command.refundedAt);
    }
}
