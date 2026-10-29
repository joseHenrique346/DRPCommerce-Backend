using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
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

    /// <summary>
    /// Consulta o pedido com itens, cobrança e histórico de status.
    /// </summary>
    [HttpGet("{id:long}/detail")]
    public async Task<IActionResult> GetDetailAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new GetOrderDetailQuery(id), ct));
    }

    /// <summary>
    /// Registra o pagamento recebido (confirmação manual enquanto não há gateway). Baixa o estoque e conta o uso do cupom.
    /// </summary>
    [HttpPost("{id:long}/confirm-payment")]
    public async Task<IActionResult> ConfirmPaymentAsync(long id, [FromBody] ConfirmPaymentRequest? request, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new ConfirmOrderPaymentCommand(id, request?.TransactionMethodId,
            request?.GatewayReference, request?.Note), ct));
    }

    /// <summary>
    /// Confirmado → Em processamento.
    /// </summary>
    [HttpPost("{id:long}/start-processing")]
    public async Task<IActionResult> StartProcessingAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new AdvanceOrderCommand(id, OrderStep.StartProcessing, null), ct));
    }

    /// <summary>
    /// Em processamento → Enviado.
    /// </summary>
    [HttpPost("{id:long}/ship")]
    public async Task<IActionResult> ShipAsync(long id, [FromBody] ShipOrderRequest? request, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new AdvanceOrderCommand(id, OrderStep.Ship, request?.Note), ct));
    }

    /// <summary>
    /// Enviado → Entregue.
    /// </summary>
    [HttpPost("{id:long}/deliver")]
    public async Task<IActionResult> DeliverAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new AdvanceOrderCommand(id, OrderStep.Deliver, null), ct));
    }

    /// <summary>
    /// Cancela pedidos pendentes, confirmados ou em processamento. Devolve estoque e cupom e estorna o pagamento, se houver.
    /// Exige a permissão orders.cancel.
    /// </summary>
    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> CancelAsync(long id, [FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new CancelOrderCommand(id, request.Reason), ct));
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
