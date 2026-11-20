using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class AccessContractsApiTests(ContractsApiFixture fixture)
{
    private const string Contract = "/api/enterprises/current/contract";

    [PostgresFact]
    public async Task Free_owner_remains_accessible_but_cannot_invite_or_register_custom_domain()
    {
        var owner = await fixture.CreateStoreAsync();
        using var client = fixture.Client(owner.Token);
        using var invitation = await client.PostAsJsonAsync("/api/invitations", Invite());
        using var domain = await client.PostAsJsonAsync("/api/enterprise-domains/add", new { host = "test.example.invalid" });
        Assert.Equal(HttpStatusCode.Conflict, invitation.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, domain.StatusCode);
        using var contract = await client.GetAsync(Contract);
        contract.EnsureSuccessStatusCode();
    }

    [PostgresFact]
    public async Task Concurrent_invitations_reserve_only_one_available_slot()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        using var client = fixture.Client(owner.Token);
        var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(Index => client.PostAsJsonAsync("/api/invitations", Invite())));

        try
        {
            Assert.Single(responses, Response => Response.IsSuccessStatusCode);
            Assert.Equal(3, responses.Count(Response => Response.StatusCode == HttpStatusCode.Conflict));
        }
        finally
        {
            foreach (var response in responses)
                response.Dispose();
        }

        using var usage = await client.GetAsync(Contract + "/usage");
        usage.EnsureSuccessStatusCode();
        using var json = await JsonAsync(usage);
        Assert.Equal(1, json.RootElement.GetProperty("usage").GetProperty("pendingInvitations").GetInt64());
        Assert.Equal(0, json.RootElement.GetProperty("remainingEmployeeSlots").GetInt64());
    }

    [PostgresFact]
    public async Task Accepting_a_valid_invitation_converts_its_slot_without_double_counting()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        var guest = await fixture.CreateUserAsync();
        using var client = fixture.Client(owner.Token);
        using var invitation = await client.PostAsJsonAsync("/api/invitations", Invite(guest.Email));
        invitation.EnsureSuccessStatusCode();
        using var link = await JsonAsync(invitation);
        using var acceptor = fixture.Client(guest.Token);
        using var accepted = await acceptor.PostAsJsonAsync("/api/identity/invitations/accept", new { token = link.RootElement.GetProperty("token").GetString() });
        accepted.EnsureSuccessStatusCode();
        using var json = await JsonAsync(accepted);
        Assert.False(json.RootElement.GetProperty("isContractBlocked").GetBoolean());
        using var full = await client.PostAsJsonAsync("/api/invitations", Invite());
        Assert.Equal(HttpStatusCode.Conflict, full.StatusCode);
    }

    [PostgresFact]
    public async Task Replacing_or_revoking_an_invitation_does_not_leak_reserved_capacity()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        using var client = fixture.Client(owner.Token);
        var email = Guid.NewGuid().ToString("N") + "@example.invalid";
        using var first = await client.PostAsJsonAsync("/api/invitations", Invite(email));
        first.EnsureSuccessStatusCode();
        using var replacement = await client.PostAsJsonAsync("/api/invitations", Invite(email));
        replacement.EnsureSuccessStatusCode();
        using var link = await JsonAsync(replacement);
        using var revoked = await client.DeleteAsync("/api/invitations/" + link.RootElement.GetProperty("id").GetInt64());
        Assert.Equal(HttpStatusCode.NoContent, revoked.StatusCode);
        using var available = await client.PostAsJsonAsync("/api/invitations", Invite());
        available.EnsureSuccessStatusCode();
    }

    [PostgresFact]
    public async Task Old_invitation_revalidates_capacity_after_downgrade_and_works_again_after_upgrade()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        var guest = await fixture.CreateUserAsync();
        using var client = fixture.Client(owner.Token);
        using var invitation = await client.PostAsJsonAsync("/api/invitations", Invite(guest.Email));
        invitation.EnsureSuccessStatusCode();
        using var json = await JsonAsync(invitation);
        var token = json.RootElement.GetProperty("token").GetString();
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        using var acceptor = fixture.Client(guest.Token);
        using var denied = await acceptor.PostAsJsonAsync("/api/identity/invitations/accept", new { token });
        Assert.Equal(HttpStatusCode.Conflict, denied.StatusCode);
        await fixture.ChangeContractAsync(owner.EnterpriseId, 4, null);
        using var allowed = await acceptor.PostAsJsonAsync("/api/identity/invitations/accept", new { token });
        allowed.EnsureSuccessStatusCode();
    }

    [PostgresFact]
    public async Task Downgrade_requires_owner_selection_and_persists_it_in_the_idempotent_request()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var first = await fixture.CreateStaffAsync(owner.EnterpriseId, kindId: 2);
        await fixture.CreateStaffAsync(owner.EnterpriseId);
        using var client = fixture.Client(owner.Token);
        using var missing = await client.PostAsJsonAsync(Contract + "/requests", Change("missing", null));
        Assert.Equal(HttpStatusCode.Conflict, missing.StatusCode);
        using var selected = await client.PostAsJsonAsync(Contract + "/requests", Change("selected", [first.MembershipId]));
        Assert.Equal(HttpStatusCode.Accepted, selected.StatusCode);
        using var snapshot = await JsonAsync(selected);
        Assert.Equal(first.MembershipId, snapshot.RootElement.GetProperty("retainedMembershipIds")[0].GetInt64());
        using var repeat = await client.PostAsJsonAsync(Contract + "/requests", Change("selected", [first.MembershipId]));
        Assert.Equal(HttpStatusCode.Accepted, repeat.StatusCode);
        using var conflict = await client.PostAsJsonAsync(Contract + "/requests", Change("selected", []));
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.All(await MembersAsync(owner.EnterpriseId), Membership => Assert.False(Membership.IsContractBlocked));
    }

    [PostgresFact]
    public async Task Voluntary_downgrade_keeps_the_selected_newer_member_and_preserves_other_store_access()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var oldest = await fixture.CreateStaffAsync(owner.EnterpriseId, kindId: 2);
        var newest = await fixture.CreateStaffAsync(owner.EnterpriseId);
        var otherStore = await fixture.CreateAnotherStoreForUserAsync(oldest);
        await fixture.ChangeContractAsync(owner.EnterpriseId, 4, null, [newest.MembershipId]);
        var members = await MembersAsync(owner.EnterpriseId);
        Assert.True(members.Single(Membership => Membership.Id == oldest.MembershipId).IsContractBlocked);
        Assert.False(members.Single(Membership => Membership.Id == newest.MembershipId).IsContractBlocked);
        Assert.All(members, Membership => Assert.True(Membership.IsActive));
        using var blocked = fixture.Client(oldest.Token);
        using var invalid = await blocked.GetAsync(Contract);
        Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
        await Assert.ThrowsAsync<SecurityFailure>(() => fixture.FreshTokenAsync(oldest));
        using var independent = fixture.Client(otherStore.Token);
        using var unaffected = await independent.GetAsync(Contract);
        unaffected.EnsureSuccessStatusCode();
    }

    [PostgresFact]
    public async Task Upgrade_restores_only_members_blocked_by_contract_not_members_disabled_by_owner()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var first = await fixture.CreateStaffAsync(owner.EnterpriseId);
        var disabled = await fixture.CreateStaffAsync(owner.EnterpriseId);
        var restored = await fixture.CreateStaffAsync(owner.EnterpriseId, kindId: 2);
        await fixture.ChangeContractAsync(owner.EnterpriseId, 4, null, [first.MembershipId]);
        using var client = fixture.Client(await fixture.FreshTokenAsync(owner));
        using var deactivated = await client.PutAsJsonAsync($"/api/enterprise-memberships/{disabled.MembershipId}", new { kindId = 3, isActive = false });
        deactivated.EnsureSuccessStatusCode();
        await fixture.ChangeContractAsync(owner.EnterpriseId, 2, null);
        var members = await MembersAsync(owner.EnterpriseId);
        Assert.False(members.Single(Membership => Membership.Id == disabled.MembershipId).IsActive);
        Assert.False(members.Single(Membership => Membership.Id == disabled.MembershipId).IsContractBlocked);
        Assert.False(members.Single(Membership => Membership.Id == restored.MembershipId).IsContractBlocked);
        using var employee = fixture.Client(await fixture.FreshTokenAsync(restored));
        using var contract = await employee.GetAsync(Contract);
        contract.EnsureSuccessStatusCode();
    }

    [PostgresFact]
    public async Task Trusted_grant_refuses_reduction_without_selection_and_rolls_back_product_effects()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        await fixture.CreateStaffAsync(owner.EnterpriseId);
        await fixture.CreateStaffAsync(owner.EnterpriseId);
        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => fixture.ChangeContractAsync(owner.EnterpriseId, 4, null));
        Assert.Equal("Team.SelectionRequired", failure.Code);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        Assert.Equal(2, (await db.Enterprises.IgnoreQueryFilters().SingleAsync(Enterprise => Enterprise.Id == owner.EnterpriseId)).PlanId);
        Assert.False(await db.Products.IgnoreQueryFilters().AnyAsync(Product => Product.EnterpriseId == owner.EnterpriseId && Product.IsContractBlocked));
    }

    [PostgresFact]
    public async Task Access_preview_shows_choices_without_deactivating_members()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var first = await fixture.CreateStaffAsync(owner.EnterpriseId);
        await fixture.CreateStaffAsync(owner.EnterpriseId, kindId: 2);
        using var client = fixture.Client(owner.Token);
        using var preview = await client.PostAsJsonAsync(Contract + "/access-impact",
            new { planId = 4, expectedContractRevision = 2, catalogVersion = 1, retainedMembershipIds = new[] { first.MembershipId } });
        preview.EnsureSuccessStatusCode();
        using var json = await JsonAsync(preview);
        Assert.True(json.RootElement.GetProperty("requiresMembershipSelection").GetBoolean());
        Assert.Single(json.RootElement.GetProperty("membershipIdsToBlock").EnumerateArray());
        Assert.All(await MembersAsync(owner.EnterpriseId), Membership => Assert.False(Membership.IsContractBlocked));
    }

    [PostgresFact]
    public async Task Domain_loss_disables_custom_host_keeps_verification_and_uses_platform_host()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2, activeProducts: 1);
        var buyer = await fixture.CreateCustomerAsync(owner.EnterpriseId);
        var custom = await CustomDomainAsync(owner.EnterpriseId);
        using var ownerClient = fixture.Client(owner.Token);
        using var publish = await ownerClient.PutAsJsonAsync($"/api/enterprise-domains/{custom.Id}/publication", new { isPrimary = true, isActive = true });
        Assert.Equal(HttpStatusCode.NoContent, publish.StatusCode);
        using var customer = fixture.Client(buyer.Token);
        customer.BaseAddress = new Uri("https://" + custom.Host);
        using var before = await customer.GetAsync("/api/storefront/cart");
        before.EnsureSuccessStatusCode();
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        using var denied = await customer.GetAsync("/api/storefront/cart");
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
        using var platformCustomer = fixture.Client(buyer.Token);
        platformCustomer.BaseAddress = buyer.Address;
        using var fallback = await platformCustomer.GetAsync("/api/storefront/cart");
        fallback.EnsureSuccessStatusCode();
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var domain = await db.EnterpriseDomains.IgnoreQueryFilters().SingleAsync(Domain => Domain.Id == custom.Id);
        Assert.True(domain.IsVerified && domain.IsContractBlocked);
        Assert.False(domain.IsActive || domain.IsPrimary);
        Assert.True(await db.EnterpriseDomains.IgnoreQueryFilters().AnyAsync(Domain =>
            Domain.EnterpriseId == owner.EnterpriseId && Domain.IsPrimary && Domain.NormalizedHost.EndsWith(".shops.example.invalid")));
    }

    [PostgresFact]
    public async Task Upgrade_does_not_automatically_republish_custom_domain_and_owner_can_reactivate_it()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var custom = await CustomDomainAsync(owner.EnterpriseId);
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        await fixture.ChangeContractAsync(owner.EnterpriseId, 2, null);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        Assert.False((await db.EnterpriseDomains.IgnoreQueryFilters().SingleAsync(Domain => Domain.Id == custom.Id)).IsActive);
        using var client = fixture.Client(await fixture.FreshTokenAsync(owner));
        using var activated = await client.PutAsJsonAsync($"/api/enterprise-domains/{custom.Id}/publication", new { isPrimary = true, isActive = true });
        Assert.Equal(HttpStatusCode.NoContent, activated.StatusCode);
        var access = scope.ServiceProvider.GetRequiredService<IEnterpriseDomainAccess>();
        Assert.Equal(owner.EnterpriseId, await access.ResolveAsync(custom.Host, null, default));
    }

    [PostgresFact]
    public async Task Domain_resolution_rechecks_contract_even_without_a_persisted_block()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var custom = await CustomDomainAsync(owner.EnterpriseId);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE empresa SET plan_id = 1, contract_revision = contract_revision + 1 WHERE id = {owner.EnterpriseId}");
        var access = scope.ServiceProvider.GetRequiredService<IEnterpriseDomainAccess>();
        Assert.Null(await access.ResolveAsync(custom.Host, null, default));
        Assert.True((await db.EnterpriseDomains.IgnoreQueryFilters().SingleAsync(Domain => Domain.Id == custom.Id)).IsActive);
    }

    [PostgresFact]
    public async Task Activation_cannot_bypass_team_quota_or_select_a_foreign_member()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var first = await fixture.CreateStaffAsync(owner.EnterpriseId);
        var blocked = await fixture.CreateStaffAsync(owner.EnterpriseId);
        var foreign = await fixture.CreateStoreAsync();
        await fixture.ChangeContractAsync(owner.EnterpriseId, 4, null, [first.MembershipId]);
        using var client = fixture.Client(await fixture.FreshTokenAsync(owner));
        using var activation = await client.PutAsJsonAsync($"/api/enterprise-memberships/{blocked.MembershipId}", new { kindId = 2, isActive = true });
        Assert.Equal(HttpStatusCode.Conflict, activation.StatusCode);
        using var selection = await client.PostAsJsonAsync(Contract + "/requests",
            new { idempotencyKey = "foreign", planId = 1, expectedContractRevision = 3, catalogVersion = 1, retainedMembershipIds = new[] { foreign.MembershipId } });
        Assert.Equal(HttpStatusCode.BadRequest, selection.StatusCode);
    }

    [PostgresFact]
    public async Task Expired_invitation_releases_its_reserved_slot()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        using var client = fixture.Client(owner.Token);
        using var invitation = await client.PostAsJsonAsync("/api/invitations", Invite());
        invitation.EnsureSuccessStatusCode();
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var expired = DateTime.UtcNow.AddMinutes(-1);
        await db.EnterpriseInvitations.IgnoreQueryFilters().Where(Invitation => Invitation.EnterpriseId == owner.EnterpriseId)
            .ExecuteUpdateAsync(Setters => Setters.SetProperty(Invitation => Invitation.ExpiresAt, expired));
        using var replacement = await client.PostAsJsonAsync("/api/invitations", Invite());
        replacement.EnsureSuccessStatusCode();
    }

    [PostgresFact]
    public async Task Owner_activation_cannot_take_a_slot_reserved_by_a_valid_invitation()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        var member = await fixture.CreateStaffAsync(owner.EnterpriseId);
        using var client = fixture.Client(owner.Token);
        using var disabled = await client.PutAsJsonAsync($"/api/enterprise-memberships/{member.MembershipId}", new { kindId = 3, isActive = false });
        disabled.EnsureSuccessStatusCode();
        using var invitation = await client.PostAsJsonAsync("/api/invitations", Invite());
        invitation.EnsureSuccessStatusCode();
        using var activated = await client.PutAsJsonAsync($"/api/enterprise-memberships/{member.MembershipId}", new { kindId = 3, isActive = true });
        Assert.Equal(HttpStatusCode.Conflict, activated.StatusCode);
    }

    [PostgresFact]
    public async Task Ownership_transfer_at_exact_capacity_keeps_one_additional_access()
    {
        const string password = "Contracts-test-Password-123!";
        var owner = await fixture.CreateStoreAsync(planId: 4);
        var recipient = await fixture.CreateStaffAsync(owner.EnterpriseId);
        await SetPasswordAsync(owner.UserId, password);
        await SetPasswordAsync(recipient.UserId, password);
        using var ownerClient = fixture.Client(await fixture.FreshTokenAsync(owner));
        using var request = await ownerClient.PostAsJsonAsync("/api/ownership-transfers",
            new { recipientMembershipId = recipient.MembershipId, currentPassword = password });
        request.EnsureSuccessStatusCode();
        using var link = await JsonAsync(request);
        using var recipientClient = fixture.Client(await fixture.FreshTokenAsync(recipient));
        using var accepted = await recipientClient.PostAsJsonAsync("/api/identity/ownership-transfers/accept",
            new { token = link.RootElement.GetProperty("token").GetString(), currentPassword = password });
        Assert.Equal(HttpStatusCode.NoContent, accepted.StatusCode);
        var members = await MembersAsync(owner.EnterpriseId);
        Assert.Equal(recipient.MembershipId, members.Single(Membership => Membership.KindId == 1).Id);
        Assert.Equal(2, members.Single(Membership => Membership.Id == owner.MembershipId).KindId);
        Assert.Single(members, Membership => Membership.KindId != 1 && Membership.IsActive && !Membership.IsContractBlocked);
    }

    [PostgresFact]
    public async Task Internal_drop_resolution_and_customer_session_validation_reject_lost_custom_domain()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var buyer = await fixture.CreateCustomerAsync(owner.EnterpriseId);
        var custom = await CustomDomainAsync(owner.EnterpriseId);
        using var scope = fixture.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SharedCommerce.Application.Options.SecurityOptions>>().Value;
        var sessions = scope.ServiceProvider.GetRequiredService<StoreCommerce.Security.Services.JwtSessionService>();
        var principal = sessions.ValidateSignature(buyer.Token);
        Assert.True(await sessions.ValidateAsync(principal, custom.Host, default));
        using var client = fixture.Client();
        client.DefaultRequestHeaders.Add("X-Service-Key", options.InternalServiceKey);
        using var before = await client.GetAsync("/api/internal/tenants/resolve?host=" + custom.Host);
        before.EnsureSuccessStatusCode();
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        using var after = await client.GetAsync("/api/internal/tenants/resolve?host=" + custom.Host);
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
        Assert.False(await sessions.ValidateAsync(principal, custom.Host, default));
        Assert.True(await sessions.ValidateAsync(principal, buyer.Address.Host, default));
    }

    [PostgresFact]
    public async Task Legacy_excess_without_block_flags_cannot_issue_a_selected_staff_session_but_keeps_owner_access()
    {
        var owner = await fixture.CreateStoreAsync(planId: 2);
        var member = await fixture.CreateStaffAsync(owner.EnterpriseId);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE empresa SET plan_id = 1, contract_revision = contract_revision + 1 WHERE id = {owner.EnterpriseId}");
        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => fixture.FreshTokenAsync(member));
        Assert.Equal("Team.SelectionRequired", failure.Code);
        using var client = fixture.Client(await fixture.FreshTokenAsync(owner));
        using var available = await client.GetAsync(Contract);
        available.EnsureSuccessStatusCode();
    }

    private async Task SetPasswordAsync(long userId, string password)
    {
        using var scope = fixture.Services.CreateScope();
        using var identity = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var user = await db.Users.SingleAsync(User => User.Id == userId);
        var hasher = scope.ServiceProvider.GetRequiredService<IAccountPasswordHasher>();
        user.SetPassword(hasher.Hash(user, password), false);
        await db.SaveChangesAsync();
    }

    private async Task<List<EnterpriseMembership>> MembersAsync(long tenant)
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();

        return await db.EnterpriseMemberships.IgnoreQueryFilters().AsNoTracking()
            .Where(Membership => Membership.EnterpriseId == tenant).OrderBy(Membership => Membership.Id).ToListAsync();
    }

    private async Task<(long Id, string Host)> CustomDomainAsync(long tenant)
    {
        using var scope = fixture.Services.CreateScope();
        using var identity = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var host = "shop-" + Guid.NewGuid().ToString("N") + ".example.invalid";
        var domain = EnterpriseDomain.Create(tenant, host, false, true);
        domain.Verify();
        db.EnterpriseDomains.Add(domain);
        await db.SaveChangesAsync();

        return (domain.Id, host);
    }

    private static object Invite(string? email = null)
    {
        return new { email = email ?? Guid.NewGuid().ToString("N") + "@example.invalid", kindId = 3, accessRoleIds = Array.Empty<long>() };
    }

    private static object Change(string key, IReadOnlyList<long>? retained)
    {
        return new { idempotencyKey = key, planId = 4, expectedContractRevision = 2, catalogVersion = 1, retainedMembershipIds = retained };
    }

    private static async Task<JsonDocument> JsonAsync(HttpResponseMessage response)
    {
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
