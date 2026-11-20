using System.Globalization;
using System.Text.Json;
using DropCommerce.Application;
using DropCommerce.Application.Integrations.Store.Contracts;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Application.Security;
using DropCommerce.Infrastructure.Data;
using DropCommerce.Infrastructure.Data.Context;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Refit;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Features.Commands;
using SharedCommerce.Application.Options;
using SharedCommerce.Application.Security;
using StoreCommerce.Security.Services;

namespace StoreCommerce.Contracts.Tests;

internal sealed class DropAppearanceTestScope : IAsyncDisposable
{
    private readonly string databaseName = "drp_drop_appearance_" + Guid.NewGuid().ToString("N")[..12];
    private string adminConnection = string.Empty;
    private ServiceProvider? provider;
    private IServiceScope? scope;
    private HttpClient? storeClient;
    private bool created;
    public IServiceProvider Services => scope!.ServiceProvider;
    public IMediator Mediator => Services.GetRequiredService<IMediator>();

    public async Task InitializeAsync(ContractsApiFixture store, StoreActor owner)
    {
        var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("DRP_CONTRACTS_TEST_POSTGRES"));

        if (connection.Host is not ("localhost" or "127.0.0.1" or "::1"))
            throw new InvalidOperationException("O teste DROP exige PostgreSQL local.");

        connection.Database = "postgres";
        connection.Pooling = false;
        adminConnection = connection.ConnectionString;
        await using (var admin = new NpgsqlConnection(adminConnection))
        {
            await admin.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
            await command.ExecuteNonQueryAsync();
            created = true;
        }

        connection.Database = databaseName;
        using var storeScope = store.Services.CreateScope();
        var options = storeScope.ServiceProvider.GetRequiredService<IOptions<SecurityOptions>>().Value;
        var principal = storeScope.ServiceProvider.GetRequiredService<JwtSessionService>().ValidateSignature(await store.FreshTokenAsync(owner));
        var context = new DefaultHttpContext { User = principal };
        context.Items[SecurityNames.ResolvedTenant] = owner.EnterpriseId;
        storeClient = store.Client();
        storeClient.DefaultRequestHeaders.Add("X-Service-Key", options.InternalServiceKey);
        storeClient.DefaultRequestHeaders.Add("X-Enterprise-Id", owner.EnterpriseId.ToString(CultureInfo.InvariantCulture));
        var services = new ServiceCollection();
        services.AddLogging(Logging => Logging.ClearProviders());
        services.AddSingleton<IHttpContextAccessor>(new LocalAccessor { HttpContext = context });
        services.AddSingleton<IOptions<SecurityOptions>>(Options.Create(options));
        services.AddScoped<IDropOperationScope, ClosedOperation>();
        services.AddScoped<IStoreCustomerReader, EmptyStoreReads>();
        services.AddScoped<IStoreProductReader, EmptyStoreReads>();
        services.AddSingleton<IStoreEnterpriseRefitApi>(RestService.For<IStoreEnterpriseRefitApi>(storeClient,
            new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer() }));
        services.AddDropApplication();
        services.AddDropInfrastructure(connection.ConnectionString);
        provider = services.BuildServiceProvider();
        scope = provider.CreateScope();
        var db = Services.GetRequiredService<DropCommerceDbContext>();
        await db.Database.MigrateAsync();
        context.Request.Path = "/api/internal/appearance-themes";
        context.Request.Headers["X-Service-Key"] = options.InternalServiceKey;
        var definitions = Enumerable.Range(1, 11).Select(Position =>
            JsonSerializer.Deserialize<InputThemeDefinition>(JsonSerializer.Serialize(AppearanceTestCatalog.Definition(Position, "drop")),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))!).ToArray();
        await Mediator.Send(new RegisterAppearanceThemesCommand(new InputThemeCatalog(definitions)));
        context.Request.Path = "/api/drop/appearances";
        context.Request.Headers.Remove("X-Service-Key");
    }

    public async ValueTask DisposeAsync()
    {
        scope?.Dispose();

        if (provider is not null)
            await provider.DisposeAsync();

        storeClient?.Dispose();

        if (!created)
            return;

        if (!databaseName.StartsWith("drp_drop_appearance_", StringComparison.Ordinal) ||
            databaseName.Length != "drp_drop_appearance_".Length + 12)
            throw new InvalidOperationException("Recusada remoção fora do banco descartável DROP.");

        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE \"{databaseName}\" WITH (FORCE)", admin);
        await command.ExecuteNonQueryAsync();
        created = false;
    }

    private sealed class LocalAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private sealed class ClosedOperation : IDropOperationScope
    {
        public bool IsOpen => false;

        public IDisposable Begin()
        {
            throw new InvalidOperationException("O teste não abre fluxos de compra DROP.");
        }
    }

    private sealed class EmptyStoreReads : IStoreCustomerReader, IStoreProductReader
    {
        Task<StoreCustomerData?> IStoreCustomerReader.GetByIdAsync(long id, long tenant, CancellationToken ct)
        {
            return Task.FromResult<StoreCustomerData?>(null);
        }

        Task<IEnumerable<StoreCustomerData>> IStoreCustomerReader.GetListByListIdAsync(List<long> ids, CancellationToken ct)
        {
            return Task.FromResult<IEnumerable<StoreCustomerData>>([]);
        }

        Task<StoreProductData?> IStoreProductReader.GetByIdAsync(long id, long tenant, CancellationToken ct)
        {
            return Task.FromResult<StoreProductData?>(null);
        }

        Task<IEnumerable<StoreProductData>> IStoreProductReader.GetListByListIdAsync(List<long> ids, CancellationToken ct)
        {
            return Task.FromResult<IEnumerable<StoreProductData>>([]);
        }
    }
}
