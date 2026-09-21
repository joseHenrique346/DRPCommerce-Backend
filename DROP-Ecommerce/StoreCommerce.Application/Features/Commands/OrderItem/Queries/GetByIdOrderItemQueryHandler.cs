using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdOrderItemQueryHandler(IRepository<OrderItem> repository)
    : BaseGetByIdHandler<GetByIdOrderItemQuery, OrderItem>(repository)
{
    protected override long GetById(GetByIdOrderItemQuery request) => request.id;
}
