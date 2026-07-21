using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/waitlist-entries")]
public class WaitlistEntryController : BaseController<WaitlistEntry, CreateWaitlistEntryCommand, CreateListWaitlistEntryCommand, UpdateWaitlistEntryCommand, UpdateListWaitlistEntryCommand>
{
    public WaitlistEntryController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListWaitlistEntryCommand WrapCreateInRange(CreateWaitlistEntryCommand command)
    {
        return new CreateListWaitlistEntryCommand(new List<CreateWaitlistEntryCommand> { command });
    }

    protected override UpdateListWaitlistEntryCommand WrapUpdateInRange(UpdateWaitlistEntryCommand command)
    {
        return new UpdateListWaitlistEntryCommand(new List<UpdateWaitlistEntryCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListWaitlistEntryCommand(ids);
    }

    protected override IRequest<Result<List<WaitlistEntry>>> GetAllQuery()
    {
        return new GetAllWaitlistEntryQuery();
    }

    protected override IRequest<Result<WaitlistEntry>> GetByIdQuery(long id)
    {
        return new GetByIdWaitlistEntryQuery(id);
    }

    protected override IRequest<Result<List<WaitlistEntry>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdWaitlistEntryQuery(ids);
    }
}
