using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-audit-logs")]
public class DropAuditLogController : BaseController<DropAuditLog, CreateDropAuditLogCommand, CreateListDropAuditLogCommand, UpdateDropAuditLogCommand, UpdateListDropAuditLogCommand>
{
    public DropAuditLogController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropAuditLogCommand WrapCreateInRange(CreateDropAuditLogCommand command)
    {
        return new CreateListDropAuditLogCommand(new List<CreateDropAuditLogCommand> { command });
    }

    protected override UpdateListDropAuditLogCommand WrapUpdateInRange(UpdateDropAuditLogCommand command)
    {
        return new UpdateListDropAuditLogCommand(new List<UpdateDropAuditLogCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropAuditLogCommand(ids);
    }

    protected override IRequest<Result<List<DropAuditLog>>> GetAllQuery()
    {
        return new GetAllDropAuditLogQuery();
    }

    protected override IRequest<Result<DropAuditLog>> GetByIdQuery(long id)
    {
        return new GetByIdDropAuditLogQuery(id);
    }

    protected override IRequest<Result<List<DropAuditLog>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropAuditLogQuery(ids);
    }
}
