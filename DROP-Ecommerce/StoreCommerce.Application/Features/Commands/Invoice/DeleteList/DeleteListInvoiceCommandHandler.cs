using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListInvoiceCommandHandler(IRepository<Invoice> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListInvoiceCommand, Invoice>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListInvoiceCommand request) => request.ids;
}
