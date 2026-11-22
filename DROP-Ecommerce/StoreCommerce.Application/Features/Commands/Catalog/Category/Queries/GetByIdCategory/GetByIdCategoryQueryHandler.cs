using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdCategoryQueryHandler(IRepository<Category> repository)
    : BaseGetByIdHandler<GetByIdCategoryQuery, Category>(repository)
{
    protected override long GetById(GetByIdCategoryQuery request) => request.id;
}
