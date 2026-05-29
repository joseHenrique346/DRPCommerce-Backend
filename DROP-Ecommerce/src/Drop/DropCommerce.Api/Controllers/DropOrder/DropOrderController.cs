using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-order")]
public class DropOrderController : BaseController<DropOrder, CreateDropOrderCommand, CreateListDropOrderCommand, UpdateDropOrderCommand, UpdateListDropOrderCommand>
{
    public DropOrderController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropOrderCommand WrapCreateInRange(CreateDropOrderCommand command)
    {
        return new CreateListDropOrderCommand(new List<CreateDropOrderCommand> { command });
    }

    protected override UpdateListDropOrderCommand WrapUpdateInRange(UpdateDropOrderCommand command)
    {
        return new UpdateListDropOrderCommand(new List<UpdateDropOrderCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropOrderCommand(ids);
    }

    protected override IRequest<Result<List<DropOrder>>> GetAllQuery()
    {
        return new GetAllDropOrderQuery();
    }

    protected override IRequest<Result<DropOrder>> GetByIdQuery(long id)
    {
        return new GetByIdDropOrderQuery(id);
    }

    protected override IRequest<Result<List<DropOrder>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropOrderQuery(ids);
    }
}
