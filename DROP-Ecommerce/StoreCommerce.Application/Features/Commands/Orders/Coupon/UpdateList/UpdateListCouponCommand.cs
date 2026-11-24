using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListCouponCommand(List<UpdateCouponCommand> commands) : IRequest<Result<List<Coupon>>> { }
