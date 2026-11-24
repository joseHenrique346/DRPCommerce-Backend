using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdServiceQueryHandler(IRepository<Service> repository)
    : BaseGetByIdHandler<GetByIdServiceQuery, Service>(repository)
{
    protected override long GetById(GetByIdServiceQuery request) => request.id;
}
