namespace DropCommerce.Application.Contracts;

public sealed record DropCustomerRegistrationResponse(
    long Id,
    long DropEventId,
    long DropRegistrationStatusId,
    bool IsEligible,
    DateTime RegisteredAt);
