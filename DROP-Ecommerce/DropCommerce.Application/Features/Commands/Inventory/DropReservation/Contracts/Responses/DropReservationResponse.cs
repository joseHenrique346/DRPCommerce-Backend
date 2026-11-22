namespace DropCommerce.Application.Contracts;

public sealed record DropReservationResponse(
    long Id,
    long DropEventId,
    long DropProductId,
    int Quantity,
    decimal TotalAmount,
    DateTime ExpiresAt,
    IEnumerable<DropReservationLineResponse> Lines);
