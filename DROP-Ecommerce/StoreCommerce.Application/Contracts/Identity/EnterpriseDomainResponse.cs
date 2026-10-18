namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record EnterpriseDomainResponse(long Id, long EnterpriseId, string Host, string NormalizedHost, bool IsPrimary, bool IsVerified, bool IsActive);
