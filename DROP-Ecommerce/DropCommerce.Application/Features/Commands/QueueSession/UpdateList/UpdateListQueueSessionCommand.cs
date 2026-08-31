using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListQueueSessionCommand(List<UpdateQueueSessionCommand> commands) : IRequest<Result<List<QueueSession>>> { }
