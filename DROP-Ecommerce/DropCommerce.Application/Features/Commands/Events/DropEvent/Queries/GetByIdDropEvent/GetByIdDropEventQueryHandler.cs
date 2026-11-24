using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropEventQueryHandler(IRepository<DropEvent> repository)
    : BaseGetByIdHandler<GetByIdDropEventQuery, DropEvent>(repository)
{
    protected override long GetById(GetByIdDropEventQuery request) => request.id;
}
