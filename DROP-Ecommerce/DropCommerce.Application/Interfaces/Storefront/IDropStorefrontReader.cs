using DropCommerce.Application.Features.Commands;

namespace DropCommerce.Application.Interfaces.Storefront;

public interface IDropStorefrontReader
{
    Task<List<DropReservationSummary>> ListMyDropReservationsAsync(CancellationToken ct);
    Task<DropReservationSummary?> GetMyDropReservationAsync(long id, CancellationToken ct);
    Task<List<DropOrderSummary>> ListMyDropOrdersAsync(CancellationToken ct);
    Task<DropOrderDetails?> GetMyDropOrderAsync(long id, CancellationToken ct);
    Task<List<DropWaitlistEntrySummary>> ListMyDropWaitlistEntriesAsync(CancellationToken ct);
    Task<List<DropNotificationSummary>> ListMyDropNotificationsAsync(CancellationToken ct);
}
