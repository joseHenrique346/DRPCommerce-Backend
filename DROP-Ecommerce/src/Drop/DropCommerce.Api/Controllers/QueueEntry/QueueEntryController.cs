using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/queue-entries")]
public class QueueEntryController : BaseController<QueueEntry, CreateQueueEntryCommand, CreateListQueueEntryCommand, UpdateQueueEntryCommand, UpdateListQueueEntryCommand>
{
    public QueueEntryController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListQueueEntryCommand WrapCreateInRange(CreateQueueEntryCommand command)
    {
        return new CreateListQueueEntryCommand(new List<CreateQueueEntryCommand> { command });
    }

    protected override UpdateListQueueEntryCommand WrapUpdateInRange(UpdateQueueEntryCommand command)
    {
        return new UpdateListQueueEntryCommand(new List<UpdateQueueEntryCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListQueueEntryCommand(ids);
    }

    protected override IRequest<Result<List<QueueEntry>>> GetAllQuery()
    {
        return new GetAllQueueEntryQuery();
    }

    protected override IRequest<Result<QueueEntry>> GetByIdQuery(long id)
    {
        return new GetByIdQueueEntryQuery(id);
    }

    protected override IRequest<Result<List<QueueEntry>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdQueueEntryQuery(ids);
    }
}
