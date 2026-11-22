using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropProductQueryHandler(IRepository<DropProduct> repository)
    : BaseGetByIdHandler<GetByIdDropProductQuery, DropProduct>(repository)
{
    protected override long GetById(GetByIdDropProductQuery request) => request.id;
}
