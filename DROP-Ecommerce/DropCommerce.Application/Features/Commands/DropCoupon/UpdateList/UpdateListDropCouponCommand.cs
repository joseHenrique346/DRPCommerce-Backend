using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListDropCouponCommand(List<UpdateDropCouponCommand> commands) : IRequest<Result<List<DropCoupon>>> { }
