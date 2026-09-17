using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdServiceQueryHandler(IRepository<Service> repository)
    : BaseGetByIdHandler<GetByIdServiceQuery, Service>(repository)
{
    protected override long GetById(GetByIdServiceQuery request) => request.id;
}
