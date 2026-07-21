using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-notifications")]
public class DropNotificationController : BaseController<DropNotification, CreateDropNotificationCommand, CreateListDropNotificationCommand, UpdateDropNotificationCommand, UpdateListDropNotificationCommand>
{
    public DropNotificationController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropNotificationCommand WrapCreateInRange(CreateDropNotificationCommand command)
    {
        return new CreateListDropNotificationCommand(new List<CreateDropNotificationCommand> { command });
    }

    protected override UpdateListDropNotificationCommand WrapUpdateInRange(UpdateDropNotificationCommand command)
    {
        return new UpdateListDropNotificationCommand(new List<UpdateDropNotificationCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropNotificationCommand(ids);
    }

    protected override IRequest<Result<List<DropNotification>>> GetAllQuery()
    {
        return new GetAllDropNotificationQuery();
    }

    protected override IRequest<Result<DropNotification>> GetByIdQuery(long id)
    {
        return new GetByIdDropNotificationQuery(id);
    }

    protected override IRequest<Result<List<DropNotification>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropNotificationQuery(ids);
    }
}
