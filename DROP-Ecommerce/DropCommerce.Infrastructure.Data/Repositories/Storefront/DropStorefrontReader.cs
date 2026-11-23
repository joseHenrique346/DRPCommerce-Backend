using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Interfaces.Storefront;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public sealed class DropStorefrontReader(DropCommerceDbContext context) : IDropStorefrontReader
{
    public Task<List<DropReservationSummary>> ListMyDropReservationsAsync(CancellationToken ct)
    {
        return context.DropReservations.AsNoTracking()
            .Select(Entity => new DropReservationSummary(Entity.Id, Entity.DropEventId, Entity.DropProductId, Entity.Quantity, Entity.TotalAmount, Entity.DropReservationStatusId, Entity.ExpiresAt))
            .ToListAsync(ct);
    }

    public Task<DropReservationSummary?> GetMyDropReservationAsync(long id, CancellationToken ct)
    {
        return context.DropReservations.AsNoTracking()
            .Where(Entity => Entity.Id == id)
            .Select(Entity => new DropReservationSummary(Entity.Id, Entity.DropEventId, Entity.DropProductId, Entity.Quantity, Entity.TotalAmount, Entity.DropReservationStatusId, Entity.ExpiresAt))
            .SingleOrDefaultAsync(ct);
    }

    public Task<List<DropOrderSummary>> ListMyDropOrdersAsync(CancellationToken ct)
    {
        return context.DropOrders.AsNoTracking()
            .Select(Entity => new DropOrderSummary(Entity.Id, Entity.DropEventId, Entity.TotalAmount, Entity.DropOrderStatusId, Entity.DropOrderPaymentStatusId))
            .ToListAsync(ct);
    }

    public Task<DropOrderDetails?> GetMyDropOrderAsync(long id, CancellationToken ct)
    {
        return context.DropOrders.AsNoTracking()
            .Where(Entity => Entity.Id == id)
            .Select(Entity => new DropOrderDetails(Entity.Id, Entity.TotalAmount, Entity.DropOrderStatusId, Entity.DropOrderPaymentStatusId,
            Entity.ListDropOrderItem.Select(Item => new DropOrderItemSummary(Item.ItemName, Item.Quantity, Item.UnitPrice, Item.TotalPrice)).ToList()))
            .SingleOrDefaultAsync(ct);
    }

    public Task<List<DropWaitlistEntrySummary>> ListMyDropWaitlistEntriesAsync(CancellationToken ct)
    {
        return context.WaitlistEntries.AsNoTracking()
            .Select(Entity => new DropWaitlistEntrySummary(Entity.Id, Entity.DropEventId, Entity.DropProductId, Entity.Position, Entity.WaitlistEntryStatusId))
            .ToListAsync(ct);
    }

    public Task<List<DropNotificationSummary>> ListMyDropNotificationsAsync(CancellationToken ct)
    {
        return context.DropNotifications.AsNoTracking()
            .Select(Entity => new DropNotificationSummary(Entity.Id, Entity.Subject, Entity.Body, Entity.ScheduledAt))
            .ToListAsync(ct);
    }
}
