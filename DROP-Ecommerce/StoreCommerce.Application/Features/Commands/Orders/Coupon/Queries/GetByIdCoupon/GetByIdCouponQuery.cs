using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetByIdCouponQuery(long id) : IRequest<Result<Coupon>> { }
