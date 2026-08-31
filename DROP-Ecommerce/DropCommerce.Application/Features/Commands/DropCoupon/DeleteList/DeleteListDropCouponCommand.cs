using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteListDropCouponCommand(List<long> ids) : IRequest<Result<bool>> { }
