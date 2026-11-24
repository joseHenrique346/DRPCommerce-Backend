using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdEnterpriseDomainQueryHandler(IEnterpriseDomainRepository repository)
    : BaseGetByIdHandler<GetByIdEnterpriseDomainQuery, EnterpriseDomain>(repository)
{
    protected override long GetById(GetByIdEnterpriseDomainQuery request) => request.id;
}
