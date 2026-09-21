using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdProductQueryHandler(IRepository<Product> repository)
    : BaseGetByIdHandler<GetByIdProductQuery, Product>(repository)
{
    protected override long GetById(GetByIdProductQuery request) => request.id;
}
