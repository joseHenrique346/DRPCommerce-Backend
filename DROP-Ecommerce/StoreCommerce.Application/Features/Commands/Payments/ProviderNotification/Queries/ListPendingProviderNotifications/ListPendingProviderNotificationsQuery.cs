using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListPendingProviderNotificationsQuery() : IRequest<List<ProviderNotificationSummary>>;
