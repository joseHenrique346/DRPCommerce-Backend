using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropEventQueryHandler(IRepository<DropEvent> repository)
    : BaseGetByIdHandler<GetByIdDropEventQuery, DropEvent>(repository)
{
    protected override long GetById(GetByIdDropEventQuery request) => request.id;
}
