namespace DropCommerce.Application.Contracts;

public sealed record DropRegistrationResponse(
    long Id,
    long DropRegistrationStatusId,
    bool IsEligible);
