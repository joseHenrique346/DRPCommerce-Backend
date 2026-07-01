using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropProductQueryHandler(IRepository<DropProduct> repository)
    : BaseGetByIdHandler<GetByIdDropProductQuery, DropProduct>(repository)
{
    protected override long GetById(GetByIdDropProductQuery request) => request.id;
}
