using StoreCommerce.Application.Integrations.Providers.Interfaces;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoTokenRefresher(MercadoPagoApi api, IOptions<MercadoPagoOptions> options, TimeProvider time)
    : IProviderTokenRefresher
{
    public string Provider => IntegrationProviders.MercadoPago;

    public async Task<ProviderTokens> RefreshAsync(ProviderConnection connection, ProviderTokens tokens, CancellationToken ct)
    {
        options.Value.RequireEnvironment(connection.Environment);

        if (string.IsNullOrWhiteSpace(options.Value.ClientId) || string.IsNullOrWhiteSpace(options.Value.ClientSecret))
            throw new SecurityFailure(409, "MercadoPago.ConfigurationRequired", "Configure as credenciais OAuth para renovar a autorização.");

        using var response = await api.OAuthAsync(new
        {
            client_id = options.Value.ClientId,
            client_secret = options.Value.ClientSecret,
            grant_type = "refresh_token",
            refresh_token = tokens.RefreshToken
        }, ct);
        var renewed = MercadoPagoAuthorizationService.Tokens(response.RootElement, time.GetUtcNow().UtcDateTime, tokens.RefreshToken);
        var account = await api.AccountAsync(renewed.AccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, connection.Environment);

        if (account.Id != connection.ExternalAccountId || MercadoPagoApi.Id(response.RootElement, "user_id") != account.Id)
            throw new SecurityFailure(409, "MercadoPago.AccountMismatch", "A renovação retornou outra conta.");

        return renewed;
    }
}
