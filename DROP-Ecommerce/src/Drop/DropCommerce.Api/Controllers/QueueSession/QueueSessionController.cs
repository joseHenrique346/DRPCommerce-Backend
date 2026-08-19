using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/queue-sessions")]
public class QueueSessionController : BaseController<QueueSession, CreateQueueSessionCommand, CreateListQueueSessionCommand, UpdateQueueSessionCommand, UpdateListQueueSessionCommand>
{
    public QueueSessionController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListQueueSessionCommand WrapCreateInRange(CreateQueueSessionCommand command)
    {
        return new CreateListQueueSessionCommand(new List<CreateQueueSessionCommand> { command });
    }

    protected override UpdateListQueueSessionCommand WrapUpdateInRange(UpdateQueueSessionCommand command)
    {
        return new UpdateListQueueSessionCommand(new List<UpdateQueueSessionCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListQueueSessionCommand(ids);
    }

    protected override IRequest<Result<List<QueueSession>>> GetAllQuery()
    {
        return new GetAllQueueSessionQuery();
    }

    protected override IRequest<Result<QueueSession>> GetByIdQuery(long id)
    {
        return new GetByIdQueueSessionQuery(id);
    }

    protected override IRequest<Result<List<QueueSession>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdQueueSessionQuery(ids);
    }
}
