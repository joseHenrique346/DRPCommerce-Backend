using Commerce.Appearance.Application.Features;
using Commerce.Appearance.Domain;
using DropCommerce.Application.Appearance;
using MediatR;

namespace DropCommerce.Application.Features;

public sealed class AppearancePublishedNotificationHandler(IDropAppearanceEventRepository events)
    : INotificationHandler<AppearancePublishedNotification>
{
    public async Task Handle(AppearancePublishedNotification notification, CancellationToken ct)
    {
        if (notification.Target.DropEventId is not long eventId)
            return;

        var dropEvent = await events.GetAsync(notification.EnterpriseId, eventId, ct)
            ?? throw new AppearanceFailure(404, "Appearance.TargetNotFound", "Evento indisponível.");

        if (dropEvent.EffectiveAppearanceRevisionId.HasValue || dropEvent.DropEventStatusId is >= 5 and <= 7)
            dropEvent.PinAppearance(notification.RevisionId);
    }
}
