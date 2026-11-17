using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Integrations;

public sealed class ProviderConnectionStore(StoreCommerceDbContext context, IntegrationWriteScope scope) : IProviderConnectionStore
{
    public Task<ProviderConnection?> GetAsync(long enterpriseId, string provider, IntegrationEnvironment environment, CancellationToken ct)
    {
        return context.ProviderConnections.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Connection => Connection.EnterpriseId == enterpriseId && Connection.Provider == provider &&
                Connection.Environment == environment, ct);
    }

    public Task<List<ProviderConnection>> ListAsync(long enterpriseId, CancellationToken ct)
    {
        return context.ProviderConnections.IgnoreQueryFilters().AsNoTracking()
            .Where(Connection => Connection.EnterpriseId == enterpriseId)
            .OrderBy(Connection => Connection.Provider).ThenBy(Connection => Connection.Environment).ToListAsync(ct);
    }

    public async Task SaveAuthorizationAsync(ProviderConnection connection, CancellationToken ct)
    {
        if (!await context.Enterprises.IgnoreQueryFilters().AnyAsync(Enterprise =>
            Enterprise.Id == connection.EnterpriseId && Enterprise.IsActive, ct))
            throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        if (connection.Id == 0)
        {
            context.ProviderConnections.Add(connection);
        }
        else
        {
            context.ProviderConnections.Update(connection);
            context.Entry(connection).Property(Connection => Connection.Version).OriginalValue = connection.Version - 1;
        }

        try
        {
            using (scope.Begin(connection.EnterpriseId))
                await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new SecurityFailure(409, "Integration.ConnectionChanged", "A conexão mudou. Repita a autorização.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            throw new SecurityFailure(409, "Integration.ConnectionExists", "A conexão já foi criada por outra autorização.");
        }
        finally
        {
            context.Entry(connection).State = EntityState.Detached;
        }
    }

    public async Task<bool> TryClaimRefreshAsync(ProviderConnection connection, Guid lease, DateTime now, CancellationToken ct)
    {
        var changed = await context.ProviderConnections.IgnoreQueryFilters()
            .Where(Connection => Connection.Id == connection.Id && Connection.EnterpriseId == connection.EnterpriseId &&
                Connection.Version == connection.Version && Connection.IsConnected &&
                (Connection.RefreshLeaseExpiresAt == null || Connection.RefreshLeaseExpiresAt <= now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Connection => Connection.RefreshLeaseToken, lease)
                .SetProperty(Connection => Connection.RefreshLeaseExpiresAt, now.AddMinutes(2)), ct);

        return changed == 1;
    }

    public async Task<bool> CompleteRefreshAsync(ProviderConnection connection, Guid lease, string protectedTokens,
        ProviderTokens tokens, DateTime now, CancellationToken ct)
    {
        var changed = await context.ProviderConnections.IgnoreQueryFilters()
            .Where(Connection => Connection.Id == connection.Id && Connection.EnterpriseId == connection.EnterpriseId &&
                Connection.Version == connection.Version && Connection.IsConnected && Connection.RefreshLeaseToken == lease)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Connection => Connection.ProtectedTokens, protectedTokens)
                .SetProperty(Connection => Connection.AccessTokenExpiresAt, tokens.AccessExpiresAt)
                .SetProperty(Connection => Connection.RefreshTokenExpiresAt, tokens.RefreshExpiresAt)
                .SetProperty(Connection => Connection.RefreshedAt, now)
                .SetProperty(Connection => Connection.RefreshLeaseToken, (Guid?)null)
                .SetProperty(Connection => Connection.RefreshLeaseExpiresAt, (DateTime?)null)
                .SetProperty(Connection => Connection.Version, Connection => Connection.Version + 1)
                .SetProperty(Connection => Connection.UpdatedAt, now), ct);

        return changed == 1;
    }

    public async Task DisconnectAsync(long enterpriseId, string provider, IntegrationEnvironment environment, DateTime now, CancellationToken ct)
    {
        await context.ProviderConnections.IgnoreQueryFilters()
            .Where(Connection => Connection.EnterpriseId == enterpriseId && Connection.Provider == provider &&
                Connection.Environment == environment && Connection.IsConnected)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Connection => Connection.IsConnected, false)
                .SetProperty(Connection => Connection.ProtectedTokens, string.Empty)
                .SetProperty(Connection => Connection.DisconnectedAt, now)
                .SetProperty(Connection => Connection.RefreshLeaseToken, (Guid?)null)
                .SetProperty(Connection => Connection.RefreshLeaseExpiresAt, (DateTime?)null)
                .SetProperty(Connection => Connection.Version, Connection => Connection.Version + 1)
                .SetProperty(Connection => Connection.UpdatedAt, now), ct);
    }
}
