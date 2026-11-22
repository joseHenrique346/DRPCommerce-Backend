namespace StoreCommerce.Application.Contracts;

public sealed record PlatformBillingNotice(string Code, string Message, DateTime? Deadline);
