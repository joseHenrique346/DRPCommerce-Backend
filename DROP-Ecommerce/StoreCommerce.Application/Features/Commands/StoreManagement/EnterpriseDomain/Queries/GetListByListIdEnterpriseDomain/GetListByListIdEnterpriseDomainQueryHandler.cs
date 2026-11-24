using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdEnterpriseDomainQueryHandler(IEnterpriseDomainRepository repository)
    : BaseGetListByListIdHandler<GetListByListIdEnterpriseDomainQuery, EnterpriseDomain>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdEnterpriseDomainQuery request) => request.ids;
}
