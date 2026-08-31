using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropOrderQueryHandler(IRepository<DropOrder> repository)
    : BaseGetByIdHandler<GetByIdDropOrderQuery, DropOrder>(repository)
{
    protected override long GetById(GetByIdDropOrderQuery request) => request.id;
}
