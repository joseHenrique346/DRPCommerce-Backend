using StoreCommerce.Application.Interfaces.Identity;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class ContractsApiTests(ContractsApiFixture fixture)
{
    private const string ContractPath = "/api/enterprises/current/contract";

    [PostgresFact]
    public async Task Catalog_is_public_and_does_not_require_a_tenant()
    {
        using var client = fixture.Client();
        using var response = await client.GetAsync("/api/enterprise-plans");
        response.EnsureSuccessStatusCode();
        using var json = await ReadAsync(response);

        Assert.Equal(5, json.RootElement.GetProperty("plans").GetArrayLength());
        Assert.Equal(5, json.RootElement.GetProperty("addOns").GetArrayLength());
        Assert.Equal(1, json.RootElement.GetProperty("maximumActiveAddOns").GetInt32());
    }

    [PostgresFact]
    public async Task Migration_preserves_the_legacy_contracts_and_has_no_pending_model_changes()
    {
        using var scope = fixture.Services.CreateScope();
        using var operation = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var ids = fixture.LegacyEnterpriseIds;
        var plans = await db.Enterprises.IgnoreQueryFilters().Where(Enterprise => ids.Contains(Enterprise.Id))
            .OrderBy(Enterprise => Enterprise.Id).Select(Enterprise => Enterprise.PlanId).ToListAsync();

        Assert.Equal(new long[] { 1, 2, 3 }, plans);
        Assert.Equal("Gratuito", (await db.EnterprisePlans.SingleAsync(Plan => Plan.Id == 1)).Description);
        Assert.Equal("Pro", (await db.EnterprisePlans.SingleAsync(Plan => Plan.Id == 2)).Description);
        Assert.Equal("DRPCommerce", (await db.EnterprisePlans.SingleAsync(Plan => Plan.Id == 3)).Description);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [PostgresFact]
    public async Task New_enterprise_starts_free_and_requires_authentication_to_read_contract()
    {
        var actor = await fixture.CreateStoreAsync();
        using var anonymous = fixture.Client();
        using var denied = await anonymous.GetAsync(ContractPath);
        Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);

        using var client = fixture.Client(actor.Token);
        using var response = await client.GetAsync(ContractPath);
        response.EnsureSuccessStatusCode();
        using var json = await ReadAsync(response);

        Assert.Equal("free", json.RootElement.GetProperty("plan").GetProperty("code").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("revision").GetInt64());
        Assert.Equal(50, json.RootElement.GetProperty("entitlements").GetProperty("storeProductLimit").GetInt32());
    }

    [PostgresFact]
    public async Task Usage_counts_inactive_products_and_co_owners_but_not_the_primary_owner()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 10, inactiveProducts: 39);
        await fixture.CreateStaffAsync(actor.EnterpriseId);
        await fixture.CreateStaffAsync(actor.EnterpriseId, kindId: 2);
        using var client = fixture.Client(actor.Token);
        using var response = await client.GetAsync(ContractPath + "/usage");
        response.EnsureSuccessStatusCode();
        using var json = await ReadAsync(response);
        var usage = json.RootElement.GetProperty("usage");

        Assert.Equal(49, usage.GetProperty("totalStoreProducts").GetInt64());
        Assert.Equal(10, usage.GetProperty("activeStoreProducts").GetInt64());
        Assert.Equal(39, usage.GetProperty("inactiveStoreProducts").GetInt64());
        Assert.Equal(2, usage.GetProperty("activeAdditionalMembers").GetInt64());
        Assert.Equal(451, json.RootElement.GetProperty("remainingStoreProducts").GetInt64());
        Assert.False(usage.GetProperty("dropUsageAvailable").GetBoolean());
        Assert.Equal(JsonValueKind.Null, usage.GetProperty("dropProducts").ValueKind);
    }

    [PostgresFact]
    public async Task Requesting_a_paid_plan_does_not_activate_or_charge_it()
    {
        var actor = await fixture.CreateStoreAsync();
        using var client = fixture.Client(actor.Token);
        using var response = await client.PostAsJsonAsync(ContractPath + "/requests", new
        {
            idempotencyKey = "paid",
            planId = 2,
            expectedContractRevision = 1,
            catalogVersion = 1,
            monthlyPrice = 0m,
            grantsAccess = true,
            enterpriseId = long.MaxValue
        });
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        using var selection = await ReadAsync(response);

        Assert.False(selection.RootElement.GetProperty("grantsAccess").GetBoolean());
        Assert.Equal("requested", selection.RootElement.GetProperty("status").GetString());
        Assert.Equal(99m, selection.RootElement.GetProperty("totalMonthlyPrice").GetDecimal());
        Assert.NotNull(response.Headers.Location);

        using var current = await client.GetAsync(ContractPath);
        current.EnsureSuccessStatusCode();
        using var contract = await ReadAsync(current);

        Assert.Equal(1, contract.RootElement.GetProperty("plan").GetProperty("id").GetInt64());
        Assert.Equal(1, contract.RootElement.GetProperty("revision").GetInt64());
    }

    [PostgresFact]
    public async Task Concurrent_identical_requests_create_a_single_intention()
    {
        var actor = await fixture.CreateStoreAsync();
        using var client = fixture.Client(actor.Token);
        var responses = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(Index => client.PostAsJsonAsync(ContractPath + "/requests", Request("same"))));
        var ids = new List<long>();

        foreach (var response in responses)
        {
            using (response)
            {
                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
                using var json = await ReadAsync(response);
                ids.Add(json.RootElement.GetProperty("id").GetInt64());
            }
        }

        Assert.Single(ids.Distinct());

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();

        Assert.Equal(1, await db.EnterpriseContractChanges.IgnoreQueryFilters()
            .CountAsync(Change => Change.EnterpriseId == actor.EnterpriseId));
    }

    [PostgresFact]
    public async Task Reusing_a_key_with_another_offer_is_a_conflict()
    {
        var actor = await fixture.CreateStoreAsync();
        using var client = fixture.Client(actor.Token);
        using var first = await client.PostAsJsonAsync(ContractPath + "/requests", Request("conflict"));
        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        using var second = await client.PostAsJsonAsync(ContractPath + "/requests", Request("conflict", planId: 3));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [PostgresFact]
    public async Task New_selection_supersedes_previous_one_without_granting_access()
    {
        var actor = await fixture.CreateStoreAsync();
        using var client = fixture.Client(actor.Token);
        using var first = await client.PostAsJsonAsync(ContractPath + "/requests", Request("first"));
        using var firstJson = await ReadAsync(first);
        var oldId = firstJson.RootElement.GetProperty("id").GetInt64();

        using var second = await client.PostAsJsonAsync(ContractPath + "/requests", Request("second", 3, 5));
        Assert.Equal(HttpStatusCode.Accepted, second.StatusCode);
        using var old = await client.GetAsync(ContractPath + "/requests/" + oldId);
        old.EnsureSuccessStatusCode();
        using var oldJson = await ReadAsync(old);

        Assert.Equal("superseded", oldJson.RootElement.GetProperty("status").GetString());
        Assert.False(oldJson.RootElement.GetProperty("grantsAccess").GetBoolean());
    }

    [PostgresFact]
    public async Task Stale_catalog_and_contract_revisions_are_rejected()
    {
        var actor = await fixture.CreateStoreAsync();
        using var client = fixture.Client(actor.Token);
        using var catalog = await client.PostAsJsonAsync(ContractPath + "/requests",
            new RequestEnterpriseContractChangeRequest("old-catalog", 2, null, 1, 99));
        using var revision = await client.PostAsJsonAsync(ContractPath + "/requests",
            new RequestEnterpriseContractChangeRequest("old-revision", 2, null, 99, 1));

        Assert.Equal(HttpStatusCode.Conflict, catalog.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, revision.StatusCode);
    }

    [PostgresFact]
    public async Task Incompatible_and_unknown_offers_are_rejected()
    {
        var actor = await fixture.CreateStoreAsync();
        using var client = fixture.Client(actor.Token);
        using var incompatible = await client.PostAsJsonAsync(ContractPath + "/requests", Request("bad-addon", 2, 5));
        using var unknown = await client.PostAsJsonAsync(ContractPath + "/requests", Request("bad-plan", 99));
        using var freeAddOn = await client.PostAsJsonAsync(ContractPath + "/requests", Request("free-addon", 1, 1));

        Assert.Equal(HttpStatusCode.BadRequest, incompatible.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, freeAddOn.StatusCode);
    }

    [PostgresFact]
    public async Task Staff_cannot_choose_offers_or_read_usage_without_settings_permission()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        var staff = await fixture.CreateStaffAsync(owner.EnterpriseId);
        using var client = fixture.Client(staff.Token);
        using var choice = await client.PostAsJsonAsync(ContractPath + "/requests", Request("staff"));
        using var usage = await client.GetAsync(ContractPath + "/usage");
        using var rights = await client.GetAsync(ContractPath);

        Assert.Equal(HttpStatusCode.Forbidden, choice.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, usage.StatusCode);
        Assert.Equal(HttpStatusCode.OK, rights.StatusCode);
    }

    [PostgresFact]
    public async Task Requests_are_isolated_by_selected_enterprise()
    {
        var first = await fixture.CreateStoreAsync();
        var second = await fixture.CreateStoreAsync();
        using var secondClient = fixture.Client(second.Token);
        using var created = await secondClient.PostAsJsonAsync(ContractPath + "/requests", Request("other-store"));
        using var json = await ReadAsync(created);
        var requestId = json.RootElement.GetProperty("id").GetInt64();
        using var firstClient = fixture.Client(first.Token);
        using var hidden = await firstClient.GetAsync(ContractPath + "/requests/" + requestId);

        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
    }

    [PostgresFact]
    public async Task Add_on_only_change_invalidates_old_selected_store_token()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2);
        using var oldClient = fixture.Client(actor.Token);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 2, 2);
        using var stale = await oldClient.GetAsync(ContractPath);
        Assert.Equal(HttpStatusCode.Unauthorized, stale.StatusCode);

        using var freshClient = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var response = await freshClient.GetAsync(ContractPath);
        response.EnsureSuccessStatusCode();
        using var json = await ReadAsync(response);

        Assert.Equal(2, json.RootElement.GetProperty("plan").GetProperty("id").GetInt64());
        Assert.Equal(8, json.RootElement.GetProperty("entitlements").GetProperty("employeeLimit").GetInt32());
    }

    [PostgresFact]
    public async Task Identity_scope_cannot_grant_a_paid_contract()
    {
        var actor = await fixture.CreateStoreAsync();
        using var scope = fixture.Services.CreateScope();
        using var identity = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var enterprise = await db.Enterprises.IgnoreQueryFilters().SingleAsync(Enterprise => Enterprise.Id == actor.EnterpriseId);
        enterprise.ChangeContract(2, null);

        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => db.SaveChangesAsync());

        Assert.Equal("Contract.GrantRequired", failure.Code);
    }

    [PostgresFact]
    public async Task Database_rejects_an_ineligible_active_add_on()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var failure = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE empresa SET add_on_id = {5L} WHERE id = {actor.EnterpriseId}"));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, failure.SqlState);
    }

    [PostgresFact]
    public async Task Changing_one_store_does_not_revoke_another_store_of_the_same_user()
    {
        var first = await fixture.CreateStoreAsync();
        var second = await fixture.CreateAnotherStoreForUserAsync(first);
        await fixture.ChangeContractAsync(first.EnterpriseId, 2, null);
        using var firstClient = fixture.Client(first.Token);
        using var secondClient = fixture.Client(second.Token);
        using var stale = await firstClient.GetAsync(ContractPath);
        using var unaffected = await secondClient.GetAsync(ContractPath);

        Assert.Equal(HttpStatusCode.Unauthorized, stale.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unaffected.StatusCode);
        using var json = await ReadAsync(unaffected);
        Assert.Equal(second.EnterpriseId, json.RootElement.GetProperty("enterpriseId").GetInt64());
        Assert.Equal(1, json.RootElement.GetProperty("plan").GetProperty("id").GetInt64());
    }

    [PostgresFact]
    public async Task Legacy_direct_upgrade_endpoint_is_not_exposed()
    {
        var actor = await fixture.CreateStoreAsync();
        using var client = fixture.Client(actor.Token);
        using var response = await client.PutAsJsonAsync("/api/enterprises/current/plan", new { planId = 3 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static RequestEnterpriseContractChangeRequest Request(string key, long planId = 2, long? addOnId = null)
    {
        return new RequestEnterpriseContractChangeRequest(key, planId, addOnId, 1, 1);
    }

    private static async Task<JsonDocument> ReadAsync(HttpResponseMessage response)
    {
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
