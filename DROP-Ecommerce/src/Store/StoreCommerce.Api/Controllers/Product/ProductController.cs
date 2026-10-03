using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/products")]
public class ProductController : BaseController<Product, CreateProductCommand, CreateListProductCommand, UpdateProductCommand, UpdateListProductCommand>
{
    public ProductController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListProductCommand WrapCreateInRange(CreateProductCommand command)
    {
        return new CreateListProductCommand(new List<CreateProductCommand> { command });
    }

    protected override UpdateListProductCommand WrapUpdateInRange(UpdateProductCommand command)
    {
        return new UpdateListProductCommand(new List<UpdateProductCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListProductCommand(ids);
    }

    protected override IRequest<Result<List<Product>>> GetAllQuery()
    {
        return new GetAllProductQuery();
    }

    protected override IRequest<Result<Product>> GetByIdQuery(long id)
    {
        return new GetByIdProductQuery(id);
    }

    protected override IRequest<Result<List<Product>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdProductQuery(ids);
    }
}
