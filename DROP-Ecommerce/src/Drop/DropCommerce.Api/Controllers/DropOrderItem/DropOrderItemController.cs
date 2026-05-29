using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-order-item")]
public class DropOrderItemController : BaseController<DropOrderItem, CreateDropOrderItemCommand, CreateListDropOrderItemCommand, UpdateDropOrderItemCommand, UpdateListDropOrderItemCommand>
{
    public DropOrderItemController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropOrderItemCommand WrapCreateInRange(CreateDropOrderItemCommand command)
    {
        return new CreateListDropOrderItemCommand(new List<CreateDropOrderItemCommand> { command });
    }

    protected override UpdateListDropOrderItemCommand WrapUpdateInRange(UpdateDropOrderItemCommand command)
    {
        return new UpdateListDropOrderItemCommand(new List<UpdateDropOrderItemCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropOrderItemCommand(ids);
    }

    protected override IRequest<Result<List<DropOrderItem>>> GetAllQuery()
    {
        return new GetAllDropOrderItemQuery();
    }

    protected override IRequest<Result<DropOrderItem>> GetByIdQuery(long id)
    {
        return new GetByIdDropOrderItemQuery(id);
    }

    protected override IRequest<Result<List<DropOrderItem>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropOrderItemQuery(ids);
    }
}
