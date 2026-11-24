using MediatR;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CompleteMelhorEnvioAuthorizationCommandHandler(
    IProviderAuthorizationStore authorizations,
    IMelhorEnvioAuthorizationClient client,
    ProviderAuthorizationSecurity security,
    ProviderAuthorizationAccessPolicy access,
    IProviderConnectionCredentials connections,
    IOptions<MelhorEnvioOptions> options,
    TimeProvider time)
    : IRequestHandler<CompleteMelhorEnvioAuthorizationCommand>
{
    public async Task Handle(CompleteMelhorEnvioAuthorizationCommand command, CancellationToken ct)
    {
        if (!CompleteMelhorEnvioAuthorizationCommandValidator.HasValidState(command))
            throw InvalidState();

        var now = time.GetUtcNow().UtcDateTime;
        var authorization = await authorizations.FindAuthorizationAsync(IntegrationProviders.MelhorEnvio, command.Environment,
            ProviderAuthorizationSecurity.Hash(command.State), ProviderAuthorizationSecurity.Hash(command.BrowserBinding), now, ct)
            ?? throw InvalidState();
        options.Value.RequireOAuth(authorization.Environment);
        security.RequireKeyRing();

        if (await authorizations.ConsumeAuthorizationAsync(authorization.Id, time.GetUtcNow().UtcDateTime, ct) != 1)
            throw InvalidState();

        if (!string.IsNullOrWhiteSpace(command.Error) || string.IsNullOrWhiteSpace(command.Code) || command.Code.Length > 4000)
            throw new SecurityFailure(400, "MelhorEnvio.AuthorizationDenied", "A autorização não foi concluída. Inicie uma nova conexão.");

        await access.RequireAsync(authorization.EnterpriseId, authorization.UserId, authorization.MembershipId, authorization.UserVersion,
            "MelhorEnvio.AuthorizationRevoked", ct);
        var account = await client.ExchangeAsync(authorization.Environment, command.Code, ct);

        await using var transaction = await authorizations.BeginSerializableTransactionAsync(ct);
        await access.RequireAsync(authorization.EnterpriseId, authorization.UserId, authorization.MembershipId, authorization.UserVersion,
            "MelhorEnvio.AuthorizationRevoked", ct);
        await connections.SaveAuthorizedAsync(authorization.EnterpriseId, IntegrationProviders.MelhorEnvio, authorization.Environment,
            account.AccountId, MelhorEnvioOptions.OAuthScopes, account.Tokens, authorization.UserId, ct);
        await transaction.CommitAsync(ct);
    }

    private static SecurityFailure InvalidState()
    {
        return new SecurityFailure(400, "MelhorEnvio.InvalidState", "A autorização expirou, já foi usada ou pertence a outro navegador ou ambiente.");
    }
}
