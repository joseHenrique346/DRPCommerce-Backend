using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/order-items")]
public class OrderItemController : BaseController<OrderItem, CreateOrderItemCommand, CreateListOrderItemCommand, UpdateOrderItemCommand, UpdateListOrderItemCommand>
{
    public OrderItemController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListOrderItemCommand WrapCreateInRange(CreateOrderItemCommand command)
    {
        return new CreateListOrderItemCommand(new List<CreateOrderItemCommand> { command });
    }

    protected override UpdateListOrderItemCommand WrapUpdateInRange(UpdateOrderItemCommand command)
    {
        return new UpdateListOrderItemCommand(new List<UpdateOrderItemCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListOrderItemCommand(ids);
    }

    protected override IRequest<Result<List<OrderItem>>> GetAllQuery()
    {
        return new GetAllOrderItemQuery();
    }

    protected override IRequest<Result<OrderItem>> GetByIdQuery(long id)
    {
        return new GetByIdOrderItemQuery(id);
    }

    protected override IRequest<Result<List<OrderItem>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdOrderItemQuery(ids);
    }
}
