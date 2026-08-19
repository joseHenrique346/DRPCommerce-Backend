using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteListQueueSessionCommand(List<long> ids) : IRequest<Result<bool>> { }
