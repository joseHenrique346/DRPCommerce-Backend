using MediatR;
using StoreCommerce.Application.Contracts.Coupon;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Simula a aplicação do cupom sobre um carrinho. CustomerId é preenchido pelo servidor (cliente autenticado na vitrine
/// ou cliente escolhido no backoffice); sem cliente, cupons com condições de cliente ou de uso único são recusados.
/// </summary>
public sealed record ValidateCouponQuery(string Code, decimal SubTotal, int ItemQuantity, long? CustomerId) : IRequest<Result<CouponValidationResponse>>;
