using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateQueueSessionCommand(long id, long queueEntryId, long customerId, string token, long statusId, DateTime issuedAt, DateTime expiresAt, DateTime lastHeartbeatAt) : IRequest<Result<QueueSession>> { }
