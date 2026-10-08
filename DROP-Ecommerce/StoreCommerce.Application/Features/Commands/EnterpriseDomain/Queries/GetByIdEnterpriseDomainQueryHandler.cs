using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdEnterpriseDomainQueryHandler(IEnterpriseDomainRepository repository)
    : BaseGetByIdHandler<GetByIdEnterpriseDomainQuery, EnterpriseDomain>(repository)
{
    protected override long GetById(GetByIdEnterpriseDomainQuery request) => request.id;
}
