using StoreCommerce.Application.Integrations.Providers.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CompleteMercadoPagoAuthorizationCommandHandler(
    IProviderAuthorizationStore authorizations,
    IMercadoPagoAuthorizationClient client,
    MercadoPagoAuthorizationCryptography secrets,
    ProviderAuthorizationAccessPolicy access,
    IProviderConnectionCredentials connections,
    IPaymentGatewayRegistry gateways,
    IOptions<MercadoPagoOptions> options,
    TimeProvider time)
    : IRequestHandler<CompleteMercadoPagoAuthorizationCommand>
{
    private MercadoPagoOptions Settings => options.Value;

    public async Task Handle(CompleteMercadoPagoAuthorizationCommand command, CancellationToken ct)
    {
        var state = command.State;
        var browserBinding = command.BrowserBinding;
        var code = command.Code;
        var error = command.Error;

        if (state.Length is < 20 or > 200 || browserBinding.Length is < 20 or > 200)
            throw InvalidState();

        var hash = MercadoPagoAuthorizationCryptography.Hash(state);
        var browserHash = MercadoPagoAuthorizationCryptography.Hash(browserBinding);
        var now = time.GetUtcNow().UtcDateTime;
        var authorization = await authorizations.FindAuthorizationAsync(hash, browserHash, now, ct) ?? throw InvalidState();
        Settings.RequireOAuth(authorization.Environment);
        secrets.RequireKeyRing();
        var consumed = await authorizations.ConsumeAuthorizationAsync(authorization.Id, now, ct);

        if (consumed != 1)
            throw InvalidState();

        if (!string.IsNullOrWhiteSpace(error) || string.IsNullOrWhiteSpace(code) || code.Length > 2000)
            throw new SecurityFailure(400, "MercadoPago.AuthorizationDenied", "A autorização não foi concluída. Inicie uma nova conexão.");

        await access.RequireAsync(authorization.EnterpriseId, authorization.UserId, authorization.MembershipId, authorization.UserVersion, ct);
        var account = await client.ExchangeAsync(code, secrets.UnprotectVerifier(authorization.ProtectedVerifier), authorization.Environment, now, ct);
        var tokens = account.Tokens;

        if (account.TokenAccountId != account.AccountId)
            throw new SecurityFailure(409, "MercadoPago.AccountMismatch", "O token e o vendedor autorizado não correspondem.");

        // A rede ficou fora desta transação. Revalida o acesso atual, inclusive exclusão/revogação ocorrida no provedor.
        await using var transaction = await authorizations.BeginSerializableTransactionAsync(ct);
        await access.RequireAsync(authorization.EnterpriseId, authorization.UserId, authorization.MembershipId, authorization.UserVersion, ct);
        await connections.SaveAuthorizedAsync(authorization.EnterpriseId, IntegrationProviders.MercadoPago, authorization.Environment,
            account.AccountId, account.Scope, tokens, authorization.UserId, ct);
        await gateways.SelectAsync(authorization.EnterpriseId, IntegrationProviders.MercadoPago, authorization.Environment, ct);
        await transaction.CommitAsync(ct);
    }

    private static SecurityFailure InvalidState()
    {
        return new SecurityFailure(400, "MercadoPago.InvalidState", "A autorização expirou, já foi usada ou pertence a outro navegador.");
    }
}
