namespace StoreCommerce.Domain.Enums.Inventory;

public enum StockMovementType
{
    Adjustment = 1,
    Reservation = 2,
    Release = 3,
    Commit = 4,
    Expiration = 5,
    Return = 6
}
