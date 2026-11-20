using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Options;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Infrastructure.Data.Context;
using StoreCommerce.Application.Identity;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class AppearanceContractsApiTests(ContractsApiFixture fixture)
{
    private const string Path = "/api/appearances";

    [PostgresFact]
    public async Task Catalog_respects_approved_model_counts_and_versions_do_not_add_positions()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);

        foreach (var (plan, count) in new (long, int)[] { (1, 1), (4, 3), (2, 10), (3, 20), (5, 0) })
        {
            var owner = await fixture.CreateStoreAsync(planId: plan);
            using var client = fixture.Client(owner.Token);
            using var catalog = await client.GetAsync("/api/appearance-themes");
            catalog.EnsureSuccessStatusCode();
            using var json = await JsonAsync(catalog);
            Assert.Equal(count, json.RootElement.EnumerateArray().Select(Theme => Theme.GetProperty("key").GetString()).Distinct().Count());
        }
    }

    [PostgresFact]
    public async Task Free_can_change_business_name_but_cannot_change_logo_colors_or_choose_paid_model()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync();
        using var client = fixture.Client(owner.Token);
        using var deniedTheme = await client.PostAsJsonAsync(Path + "/edit-sessions",
            new { themeKey = "contract-02", themeVersion = "1.0.0" });
        Assert.Equal(HttpStatusCode.Conflict, deniedTheme.StatusCode);
        var session = await BeginAsync(client, 1);
        using var deniedColor = await SendAsync(client, HttpMethod.Put, SettingsPath(session.Id),
            session.Version, new { settings = AppearanceTestCatalog.Settings(accent: "#ffffff") });
        Assert.Equal(HttpStatusCode.Conflict, deniedColor.StatusCode);
        session = await SaveAsync(client, session, AppearanceTestCatalog.Settings(name: "Minha loja"));
        using var published = await SendAsync(client, HttpMethod.Post, PublishPath(session.Id), session.Version);
        published.EnsureSuccessStatusCode();
        using var json = await JsonAsync(published);
        Assert.Equal("Minha loja", json.RootElement.GetProperty("settings").GetProperty("brand").GetProperty("name").GetString());
    }

    [PostgresFact]
    public async Task Initial_rejects_full_customization_and_paid_plan_accepts_it()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var initial = await fixture.CreateStoreAsync(planId: 4);
        using var client = fixture.Client(initial.Token);
        var session = await BeginAsync(client, 3);
        using var denied = await SendAsync(client, HttpMethod.Put, SettingsPath(session.Id), session.Version,
            new { settings = AppearanceTestCatalog.Settings(accent: "#ffffff") });
        Assert.Equal(HttpStatusCode.Conflict, denied.StatusCode);
        var pro = await fixture.CreateStoreAsync(planId: 2);
        using var paid = fixture.Client(pro.Token);
        var paidSession = await BeginAsync(paid, 10);
        paidSession = await SaveAsync(paid, paidSession, AppearanceTestCatalog.Settings(accent: "#ffffff"));
        using var accepted = await SendAsync(paid, HttpMethod.Post, PublishPath(paidSession.Id), paidSession.Version);
        accepted.EnsureSuccessStatusCode();
    }

    [PostgresFact]
    public async Task Downgrade_uses_latest_eligible_model_and_preserves_paid_revision_in_admin_history()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var buyer = await fixture.CreateCustomerAsync(owner.EnterpriseId);
        using var client = fixture.Client(owner.Token);
        var first = await BeginAsync(client, 2);
        first = await SaveAsync(client, first, AppearanceTestCatalog.Settings(name: "Nome preservado", accent: "#ffffff"));
        using var firstPublished = await SendAsync(client, HttpMethod.Post, PublishPath(first.Id), first.Version);
        firstPublished.EnsureSuccessStatusCode();
        var second = await BeginAsync(client, 4);
        using var secondPublished = await SendAsync(client, HttpMethod.Post, PublishPath(second.Id), second.Version);
        secondPublished.EnsureSuccessStatusCode();
        using var secondJson = await JsonAsync(secondPublished);
        var forbiddenRevision = secondJson.RootElement.GetProperty("revisionId").GetInt64();
        await fixture.ChangeContractAsync(owner.EnterpriseId, 4, null);
        using var storefront = fixture.Client();
        storefront.BaseAddress = buyer.Address;
        using var fallback = await storefront.GetAsync("/api/storefront/appearance");
        fallback.EnsureSuccessStatusCode();
        using var json = await JsonAsync(fallback);
        Assert.Equal("contract-02", json.RootElement.GetProperty("theme").GetProperty("key").GetString());
        Assert.Equal("contract-fallback", json.RootElement.GetProperty("source").GetString());
        Assert.Equal("Nome preservado", json.RootElement.GetProperty("settings").GetProperty("brand").GetProperty("name").GetString());
        Assert.Equal("#223344", json.RootElement.GetProperty("settings").GetProperty("accent").GetString());
        using var historic = await storefront.GetAsync("/api/storefront/appearance/revisions/" + forbiddenRevision);
        Assert.Equal(HttpStatusCode.NotFound, historic.StatusCode);
        using var ownerClient = fixture.Client(await fixture.FreshTokenAsync(owner));
        using var history = await ownerClient.GetAsync(Path + "/revisions");
        history.EnsureSuccessStatusCode();
        Assert.Contains("contract-04", await history.Content.ReadAsStringAsync());
    }

    [PostgresFact]
    public async Task Downgrade_without_eligible_history_uses_platform_default_without_overwriting_revision()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var buyer = await fixture.CreateCustomerAsync(owner.EnterpriseId);
        using var client = fixture.Client(owner.Token);
        var session = await BeginAsync(client, 4);
        using var published = await SendAsync(client, HttpMethod.Post, PublishPath(session.Id), session.Version);
        published.EnsureSuccessStatusCode();
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        using var storefront = fixture.Client();
        storefront.BaseAddress = buyer.Address;
        using var fallback = await storefront.GetAsync("/api/storefront/appearance");
        fallback.EnsureSuccessStatusCode();
        using var json = await JsonAsync(fallback);
        Assert.Equal("contract-01", json.RootElement.GetProperty("theme").GetProperty("key").GetString());
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        Assert.Equal(1, await db.Set<AppearanceRevision>().IgnoreQueryFilters().CountAsync(Revision => Revision.EnterpriseId == owner.EnterpriseId));
    }

    [PostgresFact]
    public async Task Session_opened_before_downgrade_cannot_save_preview_publish_or_rollback_paid_configuration()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync(planId: 2);
        using var client = fixture.Client(owner.Token);
        var old = await BeginAsync(client, 4);
        using var published = await SendAsync(client, HttpMethod.Post, PublishPath(old.Id), old.Version);
        published.EnsureSuccessStatusCode();
        using var json = await JsonAsync(published);
        var revision = json.RootElement.GetProperty("revisionId").GetInt64();
        var draft = await BeginAsync(client, 4);
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        using var free = fixture.Client(await fixture.FreshTokenAsync(owner));
        using var save = await SendAsync(free, HttpMethod.Put, SettingsPath(draft.Id), draft.Version,
            new { settings = AppearanceTestCatalog.Settings() });
        using var preview = await SendAsync(free, HttpMethod.Post, Path + $"/edit-sessions/{draft.Id}/preview", draft.Version);
        using var publish = await SendAsync(free, HttpMethod.Post, PublishPath(draft.Id), draft.Version);
        Assert.Equal(HttpStatusCode.Conflict, save.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, preview.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, publish.StatusCode);
        using var cancel = await SendAsync(free, HttpMethod.Post, Path + $"/edit-sessions/{draft.Id}/cancel", draft.Version);
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
        using var rollback = await free.PostAsJsonAsync(Path + $"/revisions/{revision}/rollback", new { expectedRevisionId = revision });
        Assert.Equal(HttpStatusCode.Conflict, rollback.StatusCode);
    }

    [PostgresFact]
    public async Task Downgrade_changes_public_etag_even_when_model_and_revision_stay_the_same()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var buyer = await fixture.CreateCustomerAsync(owner.EnterpriseId);
        using var client = fixture.Client(owner.Token);
        var session = await BeginAsync(client, 1);
        session = await SaveAsync(client, session, AppearanceTestCatalog.Settings(accent: "#ffffff"));
        using var published = await SendAsync(client, HttpMethod.Post, PublishPath(session.Id), session.Version);
        published.EnsureSuccessStatusCode();
        using var storefront = fixture.Client();
        storefront.BaseAddress = buyer.Address;
        using var before = await storefront.GetAsync("/api/storefront/appearance");
        before.EnsureSuccessStatusCode();
        var etag = before.Headers.ETag!.Tag;
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/storefront/appearance");
        request.Headers.TryAddWithoutValidation("If-None-Match", etag);
        using var after = await storefront.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, after.StatusCode);
        Assert.NotEqual(etag, after.Headers.ETag!.Tag);
        Assert.True(after.Headers.CacheControl!.MustRevalidate);
    }

    [PostgresFact]
    public async Task Catalog_position_is_stable_across_versions_and_only_platform_can_register_it()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        using var scope = fixture.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SecurityOptions>>().Value;
        using var platform = fixture.Client();
        platform.DefaultRequestHeaders.Add("X-Service-Key", options.InternalServiceKey);
        using var bad = await platform.PostAsJsonAsync("/api/internal/appearance-themes", new { definitions = new[] {
            new { channel = "store", key = "contract-02", version = "2.0.0", name = "Tema 2", rendererContract = 1,
                schema = JsonDocument.Parse(AppearanceTestCatalog.Schema).RootElement,
                defaults = AppearanceTestCatalog.Settings(), catalogPosition = 1 } } });
        Assert.Equal(HttpStatusCode.Conflict, bad.StatusCode);
        var owner = await fixture.CreateStoreAsync();
        using var client = fixture.Client(owner.Token);
        using var forbidden = await client.PostAsJsonAsync("/api/internal/appearance-themes",
            new { definitions = new[] { AppearanceTestCatalog.Definition(1) } });
        Assert.Equal(HttpStatusCode.Unauthorized, forbidden.StatusCode);
    }

    [PostgresFact]
    public async Task Legacy_theme_migration_assigns_one_stable_position_per_key_without_changing_content()
    {
        var legacy = new ContractsApiFixture { SeedLegacyThemes = true };

        try
        {
            await legacy.InitializeAsync();
            using var scope = legacy.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            var releases = await db.Set<ThemeRelease>().OrderBy(Release => Release.Id).ToListAsync();
            Assert.Equal(new[] { 1, 1, 2 }, releases.Select(Release => Release.CatalogPosition));
            Assert.Equal(new[] { "legacy-one", "legacy-two", "legacy-three" }, releases.Select(Release => Release.Checksum));
            Assert.Equal(new[] { "1.0.0", "2.0.0", "1.0.0" }, releases.Select(Release => Release.Version));
        }
        finally
        {
            await legacy.DisposeAsync();
        }
    }

    [PostgresFact]
    public async Task Concurrent_downgrade_and_save_cannot_leave_paid_settings_public()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var buyer = await fixture.CreateCustomerAsync(owner.EnterpriseId);
        using var client = fixture.Client(owner.Token);
        var session = await BeginAsync(client, 1);
        var saving = SendAsync(client, HttpMethod.Put, SettingsPath(session.Id), session.Version,
            new { settings = AppearanceTestCatalog.Settings(accent: "#ffffff") });
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        using var saved = await saving;
        Assert.Contains(saved.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Conflict, HttpStatusCode.Unauthorized });
        using var publicClient = fixture.Client();
        publicClient.BaseAddress = buyer.Address;
        using var manifest = await publicClient.GetAsync("/api/storefront/appearance");
        await EnsureAsync(manifest);
        using var json = await JsonAsync(manifest);
        Assert.Equal("#223344", json.RootElement.GetProperty("settings").GetProperty("accent").GetString());
    }

    [PostgresFact]
    public async Task Fallback_reads_beyond_the_first_history_page()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var buyer = await fixture.CreateCustomerAsync(owner.EnterpriseId);
        using var client = fixture.Client(owner.Token);
        var first = await BeginAsync(client, 2);
        using var publication = await SendAsync(client, HttpMethod.Post, PublishPath(first.Id), first.Version);
        await EnsureAsync(publication);
        using (var scope = fixture.Services.CreateScope())
        {
            using var identity = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
            using var operation = scope.ServiceProvider.GetRequiredService<AppearanceWriteScope>().Begin(owner.EnterpriseId);
            var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync();
            var appearance = await db.Set<SharedCommerce.Domain.Entity.Appearance>().IgnoreQueryFilters()
                .SingleAsync(Appearance => Appearance.EnterpriseId == owner.EnterpriseId);
            var release = await db.Set<ThemeRelease>().SingleAsync(Release => Release.Key == "contract-04" && Release.Version == "1.0.0");
            AppearanceRevision? last = null;

            for (var index = 0; index < 105; index++)
            {
                last = AppearanceRevision.Create(owner.EnterpriseId, appearance.Id, release.Id, index + 2,
                    release.DefaultsJson, SharedCommerce.Application.Mappings.AppearanceResponseMapper.ConfigurationChecksum(release, release.DefaultsJson),
                    owner.UserId, DateTime.UtcNow, null, SharedCommerce.Domain.StaticEntity.AppearanceRevisionOrigin.Publish);
                db.Set<AppearanceRevision>().Add(last);
            }

            await db.SaveChangesAsync();
            appearance.Publish(last!.Id);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        await fixture.ChangeContractAsync(owner.EnterpriseId, 4, null);
        using var storefront = fixture.Client();
        storefront.BaseAddress = buyer.Address;
        using var response = await storefront.GetAsync("/api/storefront/appearance");
        await EnsureAsync(response);
        using var json = await JsonAsync(response);
        Assert.Equal("contract-02", json.RootElement.GetProperty("theme").GetProperty("key").GetString());
    }

    internal sealed record Edit(long Id, long Version);

    internal static async Task<Edit> BeginAsync(HttpClient client, int position)
    {
        using var response = await client.PostAsJsonAsync(Path + "/edit-sessions",
            new { themeKey = "contract-" + position.ToString("D2"), themeVersion = "1.0.0" });
        await EnsureAsync(response);
        using var json = await JsonAsync(response);

        return new Edit(json.RootElement.GetProperty("sessionId").GetInt64(), json.RootElement.GetProperty("version").GetInt64());
    }

    internal static async Task<Edit> SaveAsync(HttpClient client, Edit edit, object settings)
    {
        using var response = await SendAsync(client, HttpMethod.Put, SettingsPath(edit.Id), edit.Version, new { settings });
        await EnsureAsync(response);
        using var json = await JsonAsync(response);

        return new Edit(edit.Id, json.RootElement.GetProperty("version").GetInt64());
    }

    internal static Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string path, long version, object? payload = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("If-Match", "\"" + version + "\"");

        if (payload is not null)
            request.Content = JsonContent.Create(payload);

        return client.SendAsync(request);
    }

    internal static string SettingsPath(long id) => Path + $"/edit-sessions/{id}/settings";
    internal static string PublishPath(long id) => Path + $"/edit-sessions/{id}/publish";

    internal static async Task<JsonDocument> JsonAsync(HttpResponseMessage response)
    {
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    internal static async Task EnsureAsync(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
    }
}
