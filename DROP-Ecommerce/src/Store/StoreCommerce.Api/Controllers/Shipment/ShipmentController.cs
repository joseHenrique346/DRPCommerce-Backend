using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/shipments")]
public class ShipmentController : BaseController<Shipment, CreateShipmentCommand, CreateListShipmentCommand, UpdateShipmentCommand, UpdateListShipmentCommand>
{
    public ShipmentController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListShipmentCommand WrapCreateInRange(CreateShipmentCommand command)
    {
        return new CreateListShipmentCommand(new List<CreateShipmentCommand> { command });
    }

    protected override UpdateListShipmentCommand WrapUpdateInRange(UpdateShipmentCommand command)
    {
        return new UpdateListShipmentCommand(new List<UpdateShipmentCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListShipmentCommand(ids);
    }

    protected override IRequest<Result<List<Shipment>>> GetAllQuery()
    {
        return new GetAllShipmentQuery();
    }

    protected override IRequest<Result<Shipment>> GetByIdQuery(long id)
    {
        return new GetByIdShipmentQuery(id);
    }

    protected override IRequest<Result<List<Shipment>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdShipmentQuery(ids);
    }
}
