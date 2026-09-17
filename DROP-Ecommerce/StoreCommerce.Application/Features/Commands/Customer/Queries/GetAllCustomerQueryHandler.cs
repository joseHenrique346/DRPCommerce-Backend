using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllCustomerQueryHandler(IRepository<Customer> repository)
    : BaseGetAllHandler<GetAllCustomerQuery, Customer>(repository);
