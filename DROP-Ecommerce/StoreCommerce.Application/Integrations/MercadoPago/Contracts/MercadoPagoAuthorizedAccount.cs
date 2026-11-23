namespace StoreCommerce.Application.Integrations.MercadoPago;

public sealed record MercadoPagoAuthorizedAccount(string AccountId, string TokenAccountId, string Scope, ProviderTokens Tokens);
