namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record UserResponse(long Id, string FullName, string Email, string NormalizedEmail, bool EmailConfirmed, bool IsActive, DateTime? LastLoginAt);
