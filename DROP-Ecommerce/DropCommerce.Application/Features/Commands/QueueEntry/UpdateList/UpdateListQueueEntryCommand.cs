using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListQueueEntryCommand(List<UpdateQueueEntryCommand> commands) : IRequest<Result<List<QueueEntry>>> { }
