using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Inventory;

public sealed record StockReservationResponse(long Id, string IdempotencyKey, string Reference, string Status,
    DateTime ExpiresAt, DateTime? CompletedAt, List<StockReservationLineResponse> Items)
{
    public static StockReservationResponse FromEntity(StockReservation reservation)
    {
        return new(reservation.Id, reservation.IdempotencyKey, reservation.Reference, reservation.Status.ToString(),
            reservation.ExpiresAt, reservation.CompletedAt, reservation.Lines.OrderBy(Line => Line.ProductVariantId)
                .Select(Line => new StockReservationLineResponse(Line.ProductVariantId, Line.Quantity, Line.AllowsBackorder)).ToList());
    }
}
