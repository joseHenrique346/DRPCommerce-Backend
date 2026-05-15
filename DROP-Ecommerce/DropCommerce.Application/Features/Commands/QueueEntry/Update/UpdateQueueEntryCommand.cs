using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateQueueEntryCommand(long id, long dropEventId, long customerId, string sessionToken, int position, long statusId, string deviceFingerprint, string ipAddress, string userAgent, DateTime enteredAt, DateTime? calledAt, DateTime? expiredAt, DateTime? checkedOutAt) : IRequest<Result<QueueEntry>> { }
