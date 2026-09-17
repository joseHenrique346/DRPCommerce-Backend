using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdEnterpriseQueryHandler(IRepository<Enterprise> repository)
    : BaseGetListByListIdHandler<GetListByListIdEnterpriseQuery, Enterprise>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdEnterpriseQuery request) => request.listId;
}
