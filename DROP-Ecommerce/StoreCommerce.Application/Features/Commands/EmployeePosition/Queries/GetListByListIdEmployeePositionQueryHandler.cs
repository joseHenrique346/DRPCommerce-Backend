using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdEmployeePositionQueryHandler(IEmployeePositionRepository repository)
    : BaseGetListByListIdHandler<GetListByListIdEmployeePositionQuery, EmployeePosition>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdEmployeePositionQuery request) => request.ids;
}
