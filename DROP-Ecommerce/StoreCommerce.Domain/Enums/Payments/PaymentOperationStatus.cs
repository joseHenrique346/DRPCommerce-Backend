namespace StoreCommerce.Domain.Enums.Payments;

public enum PaymentOperationStatus
{
    Requested = 1,
    Processing = 2,
    Pending = 3,
    Succeeded = 4,
    Failed = 5,
    OutcomeUnknown = 6,
    AwaitingManualConfirmation = 7,
    ReviewRequired = 8,
    Superseded = 9
}
