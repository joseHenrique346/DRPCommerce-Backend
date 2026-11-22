using StoreCommerce.Domain.Enums.Orders;
using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Coupon;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Busca cupons por código (parcial), tipo, ativação e situação: Scheduled (agendado), Active (vigente), Expired, Exhausted (esgotado) ou Inactive.
/// </summary>
public sealed record SearchCouponsQuery(
    string? Code = null, long? TypeId = null, bool? IsActive = null, CouponAvailability? Availability = null,
    int Page = 1, int PageSize = 20) : IRequest<Result<PagedResponse<CouponResponse>>>;
