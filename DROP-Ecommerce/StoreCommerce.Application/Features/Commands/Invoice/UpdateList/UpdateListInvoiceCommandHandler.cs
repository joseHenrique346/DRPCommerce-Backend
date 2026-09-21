using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListInvoiceCommandHandler(IRepository<Invoice> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateInvoiceCommand, UpdateListInvoiceCommand, Invoice>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateInvoiceCommand> GetCommandList(UpdateListInvoiceCommand request) => request.commands;

    protected override long GetById(UpdateInvoiceCommand command) => command.id;

    protected override void ApplyChanges(Invoice entity, UpdateInvoiceCommand command)
    {
        entity.UpdateDetails(command.number, command.series, command.accessKey, command.typeId, command.statusId, command.totalAmount, command.taxAmount, command.fileUrl, command.issuedAt);
    }
}
