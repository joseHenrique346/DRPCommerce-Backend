namespace StoreCommerce.Application.Contracts;

public sealed record DropSaleAcknowledgment(long Revision, bool FulfillmentClosed, bool RequiresReview = false);
