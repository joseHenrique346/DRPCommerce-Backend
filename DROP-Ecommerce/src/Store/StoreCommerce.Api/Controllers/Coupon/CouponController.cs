using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Coupon;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/coupons")]
public class CouponController : MappedCrudController<Coupon, CouponResponse, CreateCouponCommand, CreateListCouponCommand, UpdateCouponCommand, UpdateListCouponCommand>
{
    private readonly TimeProvider _time;

    public CouponController(IMediator mediator, TimeProvider time) : base(mediator)
    {
        _time = time;
    }

    protected override CouponResponse MapEntity(Coupon entity) => CouponResponse.FromEntity(entity, _time.GetUtcNow().UtcDateTime);

    /// <summary>
    /// Busca cupons por código, tipo, ativação e situação (Scheduled, Active, Expired, Exhausted, Inactive), com paginação.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> GetSearchAsync([FromQuery] SearchCouponsQuery query, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(query, ct));
    }

    /// <summary>
    /// Simula o cupom sobre um carrinho, opcionalmente para um cliente da loja (condições de cliente e uso único).
    /// </summary>
    [HttpGet("validate")]
    public async Task<IActionResult> GetValidateAsync([FromQuery] string code, [FromQuery] decimal subTotal, [FromQuery] int itemQuantity,
        [FromQuery] long? customerId, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new ValidateCouponQuery(code, subTotal, itemQuantity, customerId), ct));
    }

    /// <summary>
    /// Lista os tipos de condição de cupom (valor do pedido, quantidade de itens, compras anteriores e total gasto do cliente).
    /// </summary>
    [HttpGet("condition-types")]
    public IActionResult GetConditionTypes() => Ok(Result<List<CouponConditionType>>.Success(
        BaseStaticEntity.GetAll<CouponConditionType>().ToList()));

    /// <summary>
    /// Lista os operadores de condição de cupom (igual, maior que, entre, maior que e menor que, etc.).
    /// </summary>
    [HttpGet("condition-operators")]
    public IActionResult GetConditionOperators() => Ok(Result<List<CouponConditionOperator>>.Success(
        BaseStaticEntity.GetAll<CouponConditionOperator>().ToList()));

    /// <summary>
    /// Ativa ou desativa um cupom.
    /// </summary>
    [HttpPut("{id:long}/activation")]
    public async Task<IActionResult> SetActiveAsync(long id, [FromBody] bool active)
    {
        var result = await _mediator.Send(new SetCouponActiveCommand(id, active));
        return HandleResult(result);
    }

    protected override CreateListCouponCommand WrapCreateInRange(CreateCouponCommand command)
    {
        return new CreateListCouponCommand(new List<CreateCouponCommand> { command });
    }

    protected override UpdateListCouponCommand WrapUpdateInRange(UpdateCouponCommand command)
    {
        return new UpdateListCouponCommand(new List<UpdateCouponCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListCouponCommand(ids);
    }

    protected override IRequest<Result<List<Coupon>>> GetAllQuery()
    {
        return new GetAllCouponQuery();
    }

    protected override IRequest<Result<Coupon>> GetByIdQuery(long id)
    {
        return new GetByIdCouponQuery(id);
    }

    protected override IRequest<Result<List<Coupon>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdCouponQuery(ids);
    }
}
