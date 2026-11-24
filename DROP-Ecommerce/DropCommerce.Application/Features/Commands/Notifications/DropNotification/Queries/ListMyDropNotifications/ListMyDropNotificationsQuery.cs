using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ListMyDropNotificationsQuery() : IRequest<List<DropNotificationSummary>>;
