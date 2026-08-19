using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-reservations")]
public class DropReservationController : BaseController<DropReservation, CreateDropReservationCommand, CreateListDropReservationCommand, UpdateDropReservationCommand, UpdateListDropReservationCommand>
{
    public DropReservationController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropReservationCommand WrapCreateInRange(CreateDropReservationCommand command)
    {
        return new CreateListDropReservationCommand(new List<CreateDropReservationCommand> { command });
    }

    protected override UpdateListDropReservationCommand WrapUpdateInRange(UpdateDropReservationCommand command)
    {
        return new UpdateListDropReservationCommand(new List<UpdateDropReservationCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropReservationCommand(ids);
    }

    protected override IRequest<Result<List<DropReservation>>> GetAllQuery()
    {
        return new GetAllDropReservationQuery();
    }

    protected override IRequest<Result<DropReservation>> GetByIdQuery(long id)
    {
        return new GetByIdDropReservationQuery(id);
    }

    protected override IRequest<Result<List<DropReservation>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropReservationQuery(ids);
    }
}
