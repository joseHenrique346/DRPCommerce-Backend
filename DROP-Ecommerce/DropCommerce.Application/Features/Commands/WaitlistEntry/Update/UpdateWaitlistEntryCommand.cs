using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateWaitlistEntryCommand(long id, long dropEventId, long? dropProductId, long customerId, int position, long statusId, bool notificationSent, DateTime joinedAt, DateTime? notifiedAt, DateTime expiresAt) : IRequest<Result<WaitlistEntry>> { }
