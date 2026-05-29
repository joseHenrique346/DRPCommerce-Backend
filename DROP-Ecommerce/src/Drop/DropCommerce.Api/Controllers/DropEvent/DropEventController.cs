using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-event")]
public class DropEventController : BaseController<DropEvent, CreateDropEventCommand, CreateListDropEventCommand, UpdateDropEventCommand, UpdateListDropEventCommand>
{
    public DropEventController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropEventCommand WrapCreateInRange(CreateDropEventCommand command)
    {
        return new CreateListDropEventCommand(new List<CreateDropEventCommand> { command });
    }

    protected override UpdateListDropEventCommand WrapUpdateInRange(UpdateDropEventCommand command)
    {
        return new UpdateListDropEventCommand(new List<UpdateDropEventCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropEventCommand(ids);
    }

    protected override IRequest<Result<List<DropEvent>>> GetAllQuery()
    {
        return new GetAllDropEventQuery();
    }

    protected override IRequest<Result<DropEvent>> GetByIdQuery(long id)
    {
        return new GetByIdDropEventQuery(id);
    }

    protected override IRequest<Result<List<DropEvent>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropEventQuery(ids);
    }
}
