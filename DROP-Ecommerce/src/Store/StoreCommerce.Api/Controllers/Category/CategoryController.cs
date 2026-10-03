using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/categories")]
public class CategoryController : BaseController<Category, CreateCategoryCommand, CreateListCategoryCommand, UpdateCategoryCommand, UpdateListCategoryCommand>
{
    public CategoryController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListCategoryCommand WrapCreateInRange(CreateCategoryCommand command)
    {
        return new CreateListCategoryCommand(new List<CreateCategoryCommand> { command });
    }

    protected override UpdateListCategoryCommand WrapUpdateInRange(UpdateCategoryCommand command)
    {
        return new UpdateListCategoryCommand(new List<UpdateCategoryCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListCategoryCommand(ids);
    }

    protected override IRequest<Result<List<Category>>> GetAllQuery()
    {
        return new GetAllCategoryQuery();
    }

    protected override IRequest<Result<Category>> GetByIdQuery(long id)
    {
        return new GetByIdCategoryQuery(id);
    }

    protected override IRequest<Result<List<Category>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdCategoryQuery(ids);
    }
}
