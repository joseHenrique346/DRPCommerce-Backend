using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteListDropOrderCommand(List<long> ids) : IRequest<Result<bool>> { }
