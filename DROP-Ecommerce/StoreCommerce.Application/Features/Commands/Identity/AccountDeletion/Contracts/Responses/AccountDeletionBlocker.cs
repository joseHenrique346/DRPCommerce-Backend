namespace StoreCommerce.Application.Identity;

public sealed record AccountDeletionBlocker(string Code, string Message, int Count);
