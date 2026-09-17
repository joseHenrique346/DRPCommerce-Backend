using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdSupplierQueryHandler(IRepository<Supplier> repository)
    : BaseGetByIdHandler<GetByIdSupplierQuery, Supplier>(repository)
{
    protected override long GetById(GetByIdSupplierQuery request) => request.id;
}
