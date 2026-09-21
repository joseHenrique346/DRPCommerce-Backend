using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdCustomerQueryHandler(IRepository<Customer> repository)
    : BaseGetByIdHandler<GetByIdCustomerQuery, Customer>(repository)
{
    protected override long GetById(GetByIdCustomerQuery request) => request.id;
}
