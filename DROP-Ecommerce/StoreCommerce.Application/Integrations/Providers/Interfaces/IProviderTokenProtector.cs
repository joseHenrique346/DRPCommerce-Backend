using StoreCommerce.Application.Integrations;
using StoreCommerce.Domain.Enums.Integrations;
using System.Text.Json.Serialization;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Integrations.Providers.Interfaces;

public interface IProviderTokenProtector
{
    string Protect(long enterpriseId, string provider, IntegrationEnvironment environment, string accountId, ProviderTokens tokens);
    ProviderTokens Unprotect(ProviderConnection connection);
}
