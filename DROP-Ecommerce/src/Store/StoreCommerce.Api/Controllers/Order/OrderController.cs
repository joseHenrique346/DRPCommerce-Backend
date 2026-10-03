using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/orders")]
public class OrderController : BaseController<Order, CreateOrderCommand, CreateListOrderCommand, UpdateOrderCommand, UpdateListOrderCommand>
{
    public OrderController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListOrderCommand WrapCreateInRange(CreateOrderCommand command)
    {
        return new CreateListOrderCommand(new List<CreateOrderCommand> { command });
    }

    protected override UpdateListOrderCommand WrapUpdateInRange(UpdateOrderCommand command)
    {
        return new UpdateListOrderCommand(new List<UpdateOrderCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListOrderCommand(ids);
    }

    protected override IRequest<Result<List<Order>>> GetAllQuery()
    {
        return new GetAllOrderQuery();
    }

    protected override IRequest<Result<Order>> GetByIdQuery(long id)
    {
        return new GetByIdOrderQuery(id);
    }

    protected override IRequest<Result<List<Order>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdOrderQuery(ids);
    }
}
