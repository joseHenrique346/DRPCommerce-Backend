using StoreCommerce.Application.Interfaces.Identity;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedCommerce.Api.Configuration;
using StoreCommerce.Api.Configuration;
using StoreCommerce.Api.Controllers;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Security.Services;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[CollectionDefinition("contracts-postgres", DisableParallelization = true)]
public sealed class ContractsPostgresCollection : ICollectionFixture<ContractsApiFixture>
{
}

public sealed class ContractsApiFixture : IAsyncLifetime
{
    private WebApplication? app;
    private string adminConnectionString = string.Empty;
    private string databaseName = string.Empty;
    private string connectionString = string.Empty;
    private bool created;
    public List<long> LegacyEnterpriseIds { get; } = [];
    public bool SeedLegacyThemes { get; init; }
    public Action<IServiceCollection>? ConfigureServices { get; init; }

    public IServiceProvider Services => app?.Services ?? throw new InvalidOperationException("Fixture não iniciada.");

    public async Task InitializeAsync()
    {
        var source = Environment.GetEnvironmentVariable("DRP_CONTRACTS_TEST_POSTGRES");

        if (string.IsNullOrWhiteSpace(source))
            return;

        var connection = new NpgsqlConnectionStringBuilder(source);

        if (connection.Host is not ("127.0.0.1" or "localhost" or "::1"))
            throw new InvalidOperationException("Os testes só criam banco descartável em PostgreSQL local.");

        databaseName = "drp_contract_checks_" + Guid.NewGuid().ToString("N")[..12];
        connection.Database = "postgres";
        connection.Pooling = false;
        adminConnectionString = connection.ConnectionString;
        await using (var admin = new NpgsqlConnection(adminConnectionString))
        {
            await admin.OpenAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
            await create.ExecuteNonQueryAsync();
            created = true;
        }

        connection.Database = databaseName;
        connectionString = connection.ConnectionString;

        try
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environments.Development,
                ApplicationName = typeof(EnterpriseContractController).Assembly.GetName().Name
            });
            builder.Logging.ClearProviders();
            builder.WebHost.UseTestServer();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Security:SigningKey"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
                ["Security:Issuer"] = "DRPCommerce.Contracts.Tests",
                ["Security:StorefrontBaseUrl"] = "https://shops.example.invalid",
                ["Security:BackofficeUrl"] = "https://backoffice.example.invalid",
                ["Security:InternalServiceKey"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
                ["Security:ApiRequestsPerMinute"] = "10000",
                ["Security:AuthRequestsPerMinute"] = "10000",
                ["MercadoPago:Enabled"] = "false",
                ["MercadoPago:AllowProduction"] = "false"
            });

            var previousConnection = Environment.GetEnvironmentVariable("STORECOMMERCE_CONNECTION_STRING");

            try
            {
                Environment.SetEnvironmentVariable("STORECOMMERCE_CONNECTION_STRING", connectionString);
                builder.Services.AddApiConfiguration(builder.Configuration);
            }
            finally
            {
                Environment.SetEnvironmentVariable("STORECOMMERCE_CONNECTION_STRING", previousConnection);
            }

            builder.Services.AddControllers().AddApplicationPart(typeof(EnterpriseContractController).Assembly);
            builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
            ConfigureServices?.Invoke(builder.Services);

            foreach (var service in builder.Services.Where(Service =>
                Service.ServiceType == typeof(IHostedService) &&
                Service.ImplementationType?.Namespace?.StartsWith("StoreCommerce.", StringComparison.Ordinal) == true).ToArray())
                builder.Services.Remove(service);

            app = builder.Build();
            app.UseExceptionHandler();
            app.UseCommerceSecurity();
            app.MapControllers();

            await PrepareMigrationsAsync();
            await app.StartAsync();
        }
        catch
        {
            await DisposeAsync();

            throw;
        }
    }

    private async Task PrepareMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261118154835_SupportMercadoPagoBalance");

        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();

            foreach (var legacyPlanId in new long[] { 1, 2, 3 })
            {
                await using var command = new NpgsqlCommand("""
                    INSERT INTO empresa
                        (trade_name, legal_name, email, phone, address_line, city, state_id, zip_code, country, is_active, plan_id, created_at)
                    VALUES
                        (@name, @name, 'legacy@example.invalid', '11999999999', 'Rua Teste', 'São Paulo', 1, '01001000', 'BR', TRUE, @plan, now())
                    RETURNING id
                    """, connection);
                command.Parameters.AddWithValue("name", "Legacy-" + legacyPlanId);
                command.Parameters.AddWithValue("plan", legacyPlanId);
                LegacyEnterpriseIds.Add((long)(await command.ExecuteScalarAsync())!);
            }
        }

        await migrator.MigrateAsync("20261119203614_EnterpriseCommercialContracts");

        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand("""
                WITH category AS (
                    INSERT INTO categoria
                        (enterprise_id, name, slug, description, image_url, display_order, is_active, created_at)
                    VALUES (@tenant, 'Legado', 'legacy', '', '', 0, TRUE, now())
                    RETURNING id
                )
                INSERT INTO produto
                    (enterprise_id, category_id, name, slug, description, sku, bar_code,
                        price, cost_price, weight, height, width, length, brand, image_urls,
                        is_active, is_digital, has_variants, created_at)
                SELECT @tenant, category.id, 'Legado ' || n, 'legacy-' || n, '', 'LEGACY-' || n, '',
                    10, 1, 1, 1, 1, 1, '', '', n <= 52, FALSE, FALSE,
                    '2026-01-01T00:00:00Z'::timestamptz
                FROM category CROSS JOIN generate_series(1, 54) n
                """, connection);
            command.Parameters.AddWithValue("tenant", LegacyEnterpriseIds[0]);
            await command.ExecuteNonQueryAsync();
        }

        if (SeedLegacyThemes)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand("""
                INSERT INTO versao_tema
                    (appearance_channel_id, key, version, name, renderer_contract, schema_json, defaults_json, checksum, migration_json)
                VALUES
                    (1, 'legacy-z', '1.0.0', 'Legado Z', 1, '{"type":"object","properties":{},"additionalProperties":false}'::jsonb, '{}'::jsonb, 'legacy-one', '{}'::jsonb),
                    (1, 'legacy-z', '2.0.0', 'Legado Z', 1, '{"type":"object","properties":{},"additionalProperties":false}'::jsonb, '{}'::jsonb, 'legacy-two', '{}'::jsonb),
                    (1, 'legacy-a', '1.0.0', 'Legado A', 1, '{"type":"object","properties":{},"additionalProperties":false}'::jsonb, '{}'::jsonb, 'legacy-three', '{}'::jsonb);
                """, connection);
            await command.ExecuteNonQueryAsync();
        }

        await migrator.MigrateAsync();
    }

    public HttpClient Client(string? token = null)
    {
        var client = app!.GetTestClient();
        client.BaseAddress = new Uri("https://backoffice.example.invalid");

        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    internal HttpMessageHandler CreateHandler()
    {
        return app!.GetTestServer().CreateHandler();
    }

    public async Task<StoreActor> CreateStoreAsync(long planId = 1, long? addOnId = null, int activeProducts = 0, int inactiveProducts = 0)
    {
        using var serviceScope = Services.CreateScope();
        var services = serviceScope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var user = User.Create("Dono", UserEmail.Create(suffix + "@example.invalid"), true);
        var enterprise = Enterprise.Create("Loja-" + suffix, "Loja Teste",
            EnterpriseEmail.Create("shop-" + suffix + "@example.invalid"),
            EnterprisePhone.Create("11999999999"), "Rua Teste", "São Paulo", 1, "01001000", "BR", true);
        enterprise.AssignStorefrontSlug("shop-" + suffix);
        db.Users.Add(user);
        db.Enterprises.Add(enterprise);
        await db.SaveChangesAsync();

        if (planId != 1 || addOnId.HasValue)
        {
            using var grant = services.GetRequiredService<EnterpriseContractWriteScope>().Begin(enterprise.Id);
            enterprise.ChangeContract(planId, addOnId);
            await db.SaveChangesAsync();
        }

        var owner = EnterpriseMembership.Create(enterprise.Id, user.Id, 1, true, DateTime.UtcNow);
        db.EnterpriseMemberships.Add(owner);
        await db.SaveChangesAsync();

        if (activeProducts + inactiveProducts > 0)
        {
            var category = Category.Create(enterprise.Id, null, "Categoria", "category-" + suffix, "", "", 0, true);
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            for (var index = 0; index < activeProducts + inactiveProducts; index++)
            {
                db.Products.Add(Product.Create(
                    enterprise.Id, category.Id, null, "Produto " + index, "product-" + suffix + "-" + index,
                    "", "SKU-" + suffix + "-" + index, "", 10m, 1m, 1m, 10m, 10m, 10m, "", "",
                    index < activeProducts, false));
            }

            await db.SaveChangesAsync();
        }

        var token = await IssueTokenAsync(services, db, user, owner);

        return new StoreActor(enterprise.Id, user.Id, owner.Id, token);
    }

    public async Task<StoreActor> CreateStaffAsync(long enterpriseId, long kindId = 3)
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var user = User.Create("Funcionário", UserEmail.Create(Guid.NewGuid().ToString("N") + "@example.invalid"), true);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var membership = EnterpriseMembership.Create(enterpriseId, user.Id, kindId, true, DateTime.UtcNow);
        db.EnterpriseMemberships.Add(membership);
        await db.SaveChangesAsync();
        var token = await IssueTokenAsync(services, db, user, membership);

        return new StoreActor(enterpriseId, user.Id, membership.Id, token);
    }

    public async Task<StoreActor> CreateAnotherStoreForUserAsync(StoreActor actor)
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var user = await db.Users.SingleAsync(User => User.Id == actor.UserId);
        var enterprise = Enterprise.Create("Outra Loja", "Outra Loja Teste",
            EnterpriseEmail.Create(Guid.NewGuid().ToString("N") + "@example.invalid"),
            EnterprisePhone.Create("11999999999"), "Rua Teste", "São Paulo", 1, "01001000", "BR", true);
        enterprise.AssignStorefrontSlug("shop-" + Guid.NewGuid().ToString("N"));
        db.Enterprises.Add(enterprise);
        await db.SaveChangesAsync();
        var membership = EnterpriseMembership.Create(enterprise.Id, user.Id, 1, true, DateTime.UtcNow);
        db.EnterpriseMemberships.Add(membership);
        await db.SaveChangesAsync();
        var token = await IssueTokenAsync(services, db, user, membership);

        return new StoreActor(enterprise.Id, user.Id, membership.Id, token);
    }

    public async Task ChangeContractAsync(long enterpriseId, long planId, long? addOnId,
        IReadOnlyList<long>? retainedMembershipIds = null)
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        using var grant = services.GetRequiredService<EnterpriseContractWriteScope>()
            .Begin(enterpriseId, retainedMembershipIds);
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var enterprise = await db.Enterprises.IgnoreQueryFilters().SingleAsync(Enterprise => Enterprise.Id == enterpriseId);
        enterprise.ChangeContract(planId, addOnId);
        await db.SaveChangesAsync();
    }

    public async Task<(Uri Address, string Token)> CreateCustomerAsync(long enterpriseId)
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var enterprise = await db.Enterprises.IgnoreQueryFilters().SingleAsync(Enterprise => Enterprise.Id == enterpriseId);

        var customer = Customer.Create(enterpriseId, "Comprador de teste",
            CustomerEmail.Create(Guid.NewGuid().ToString("N") + "@example.invalid"),
            CustomerPhone.Create("11999999999"), "Rua Teste", "São Paulo", 1, "01001000", "BR", "",
            new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), true, true);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        var family = Guid.NewGuid();
        db.CustomerRefreshTokens.Add(CustomerRefreshToken.Create(customer,
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32)), family, DateTime.UtcNow.AddDays(1)));
        await db.SaveChangesAsync();
        var token = services.GetRequiredService<JwtSessionService>().IssueCustomer(customer, family);

        return (new Uri($"https://shops.example.invalid/api/storefront/{enterprise.StorefrontSlug}/"), token.Token);
    }

    public async Task<string> FreshTokenAsync(StoreActor actor)
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var user = await db.Users.SingleAsync(User => User.Id == actor.UserId);
        var membership = await db.EnterpriseMemberships.IgnoreQueryFilters()
            .SingleAsync(Membership => Membership.Id == actor.MembershipId);

        return await IssueTokenAsync(services, db, user, membership);
    }

    public async Task<(long Id, string Email, string Token)> CreateUserAsync(string? email = null)
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var user = User.Create("Convidado", UserEmail.Create(email ?? Guid.NewGuid().ToString("N") + "@example.invalid"), true);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var token = await IssueTokenAsync(services, db, user, null);

        return (user.Id, user.Email.Value, token);
    }

    private static async Task<string> IssueTokenAsync(IServiceProvider services, StoreCommerceDbContext db,
        User user, EnterpriseMembership? membership)
    {
        var family = Guid.NewGuid();
        db.UserRefreshTokens.Add(UserRefreshToken.Create(user,
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32)), family, DateTime.UtcNow.AddDays(1), membership?.Id));
        await db.SaveChangesAsync();
        var access = await services.GetRequiredService<JwtSessionService>().IssueUserAsync(user, family, membership, default);

        return access.Token;
    }

    public async Task DisposeAsync()
    {
        if (app is not null)
        {
            await app.DisposeAsync();
            app = null;
        }

        if (!created)
            return;

        if (!databaseName.StartsWith("drp_contract_checks_", StringComparison.Ordinal) ||
            databaseName.Length != "drp_contract_checks_".Length + 12)
            throw new InvalidOperationException("Recusada remoção de banco fora do teste.");

        await using var admin = new NpgsqlConnection(adminConnectionString);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", admin);
        await drop.ExecuteNonQueryAsync();
        created = false;
    }
}

public sealed record StoreActor(long EnterpriseId, long UserId, long MembershipId, string Token);
