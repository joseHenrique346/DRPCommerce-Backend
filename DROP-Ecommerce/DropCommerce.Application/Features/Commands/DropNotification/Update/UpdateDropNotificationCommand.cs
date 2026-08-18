using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateDropNotificationCommand(long id, long dropEventId, long customerId, long channelId, long typeId, string subject, string body, long statusId, DateTime scheduledAt, DateTime? sentAt) : IRequest<Result<DropNotification>> { }
