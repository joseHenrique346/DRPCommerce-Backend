using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropOrderItemQueryHandler(IRepository<DropOrderItem> repository)
    : BaseGetByIdHandler<GetByIdDropOrderItemQuery, DropOrderItem>(repository)
{
    protected override long GetById(GetByIdDropOrderItemQuery request) => request.id;
}
