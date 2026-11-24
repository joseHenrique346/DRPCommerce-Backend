namespace StoreCommerce.Domain.Enums.Payments;

public enum PaymentCreationStatus
{
    Requested = 1,
    Processing = 2,
    Created = 3,
    OutcomeUnknown = 4,
    Aborted = 5
}
