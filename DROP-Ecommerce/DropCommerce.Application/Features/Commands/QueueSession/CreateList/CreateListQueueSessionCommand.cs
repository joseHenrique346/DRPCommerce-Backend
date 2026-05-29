using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListQueueSessionCommand(List<CreateQueueSessionCommand> commands) : IRequest<Result<List<QueueSession>>> { }
