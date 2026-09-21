using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdInvoiceQueryHandler(IRepository<Invoice> repository)
    : BaseGetByIdHandler<GetByIdInvoiceQuery, Invoice>(repository)
{
    protected override long GetById(GetByIdInvoiceQuery request) => request.id;
}
