using SharedCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteListDropOrderItemCommand(List<long> ids) : IRequest<Result<bool>> { }
