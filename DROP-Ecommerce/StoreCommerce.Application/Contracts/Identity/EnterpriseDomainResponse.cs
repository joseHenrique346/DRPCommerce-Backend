namespace StoreCommerce.Application.Contracts.Identity;

public sealed record EnterpriseDomainResponse(long Id, long EnterpriseId, string Host, string NormalizedHost, bool IsPrimary, bool IsVerified, bool IsActive);
