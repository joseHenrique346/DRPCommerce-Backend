using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdEnterpriseDomainQueryHandler(IEnterpriseDomainRepository repository)
    : BaseGetListByListIdHandler<GetListByListIdEnterpriseDomainQuery, EnterpriseDomain>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdEnterpriseDomainQuery request) => request.ids;
}
