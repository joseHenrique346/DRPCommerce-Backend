using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdCustomerQueryHandler(IRepository<Customer> repository)
    : BaseGetListByListIdHandler<GetListByListIdCustomerQuery, Customer>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdCustomerQuery request) => request.listId;
}
