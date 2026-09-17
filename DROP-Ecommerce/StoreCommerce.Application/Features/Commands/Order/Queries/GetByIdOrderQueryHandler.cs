using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdOrderQueryHandler(IRepository<Order> repository)
    : BaseGetByIdHandler<GetByIdOrderQuery, Order>(repository)
{
    protected override long GetById(GetByIdOrderQuery request) => request.id;
}
