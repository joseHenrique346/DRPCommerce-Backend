using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdEmployeeQueryHandler(IRepository<Employee> repository)
    : BaseGetListByListIdHandler<GetListByListIdEmployeeQuery, Employee>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdEmployeeQuery request) => request.listId;
}
