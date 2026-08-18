using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-products")]
public class DropProductController : BaseController<DropProduct, CreateDropProductCommand, CreateListDropProductCommand, UpdateDropProductCommand, UpdateListDropProductCommand>
{
    public DropProductController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropProductCommand WrapCreateInRange(CreateDropProductCommand command)
    {
        return new CreateListDropProductCommand(new List<CreateDropProductCommand> { command });
    }

    protected override UpdateListDropProductCommand WrapUpdateInRange(UpdateDropProductCommand command)
    {
        return new UpdateListDropProductCommand(new List<UpdateDropProductCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropProductCommand(ids);
    }

    protected override IRequest<Result<List<DropProduct>>> GetAllQuery()
    {
        return new GetAllDropProductQuery();
    }

    protected override IRequest<Result<DropProduct>> GetByIdQuery(long id)
    {
        return new GetByIdDropProductQuery(id);
    }

    protected override IRequest<Result<List<DropProduct>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropProductQuery(ids);
    }
}
