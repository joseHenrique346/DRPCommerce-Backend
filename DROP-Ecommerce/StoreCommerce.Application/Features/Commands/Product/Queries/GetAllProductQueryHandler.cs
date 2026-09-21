using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllProductQueryHandler(IRepository<Product> repository)
    : BaseGetAllHandler<GetAllProductQuery, Product>(repository);
