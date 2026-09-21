using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListInvoiceCommandHandler(IRepository<Invoice> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateInvoiceCommand, CreateListInvoiceCommand, Invoice>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateInvoiceCommand> GetCommandList(CreateListInvoiceCommand request) => request.commands;

    protected override Invoice CreateEntity(CreateInvoiceCommand command) =>
        Invoice.Create(command.orderId, command.customerId, command.enterpriseId, command.number, command.series, command.accessKey, command.typeId, command.statusId, command.totalAmount, command.taxAmount, command.fileUrl, command.issuedAt);
}
