using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-registrations")]
public class DropRegistrationController : BaseController<DropRegistration, CreateDropRegistrationCommand, CreateListDropRegistrationCommand, UpdateDropRegistrationCommand, UpdateListDropRegistrationCommand>
{
    public DropRegistrationController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropRegistrationCommand WrapCreateInRange(CreateDropRegistrationCommand command)
    {
        return new CreateListDropRegistrationCommand(new List<CreateDropRegistrationCommand> { command });
    }

    protected override UpdateListDropRegistrationCommand WrapUpdateInRange(UpdateDropRegistrationCommand command)
    {
        return new UpdateListDropRegistrationCommand(new List<UpdateDropRegistrationCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropRegistrationCommand(ids);
    }

    protected override IRequest<Result<List<DropRegistration>>> GetAllQuery()
    {
        return new GetAllDropRegistrationQuery();
    }

    protected override IRequest<Result<DropRegistration>> GetByIdQuery(long id)
    {
        return new GetByIdDropRegistrationQuery(id);
    }

    protected override IRequest<Result<List<DropRegistration>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropRegistrationQuery(ids);
    }
}
