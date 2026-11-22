using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class SetCouponActiveCommand(long id, bool active) : IRequest<Result<Coupon>> { }
