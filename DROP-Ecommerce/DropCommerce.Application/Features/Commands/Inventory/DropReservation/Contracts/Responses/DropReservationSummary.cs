namespace DropCommerce.Application.Features.Commands;

public sealed record DropReservationSummary(long Id, long DropEventId, long DropProductId, int Quantity, decimal TotalAmount, long DropReservationStatusId, DateTime ExpiresAt);
