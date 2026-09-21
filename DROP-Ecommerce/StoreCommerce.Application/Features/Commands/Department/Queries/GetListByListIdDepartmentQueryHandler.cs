using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdDepartmentQueryHandler(IRepository<Department> repository)
    : BaseGetListByListIdHandler<GetListByListIdDepartmentQuery, Department>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDepartmentQuery request) => request.listId;
}
