using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity.Coupon;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteCouponCommand(long id) : IRequest<Result<Coupon>> { }
