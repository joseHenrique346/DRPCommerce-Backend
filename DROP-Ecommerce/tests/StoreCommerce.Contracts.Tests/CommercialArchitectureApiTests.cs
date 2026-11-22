using DropCommerce.Application.Contracts;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;
using Xunit.Abstractions;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class CommercialArchitectureApiTests
{
    [PostgresFact]
    [Trait("Category", "ArchitectureRegression")]
    public async Task Storefront_registration_is_idempotent_and_isolated_after_dispatch_through_handlers()
    {
        await using var scenario = new CommercialScenario();
        await scenario.InitializeAsync();
        await scenario.ChooseAsync(3);
        await scenario.ApproveSubscriptionAsync();
        var item = await CommercialEndToEndTests.CreateEventAsync(scenario, registrationOpen: true);
        var buyer = await scenario.Store.CreateCustomerAsync(scenario.Owner.EnterpriseId);
        using var client = scenario.Client(buyer.Token, buyer.Address);
        var events = await CommercialScenario.JsonAsync(await client.GetAsync("/api/storefront/drop/events"));
        Assert.Contains(events.EnumerateArray(), Event => Event.GetProperty("id").GetInt64() == item.EventId);

        var registration = await CommercialScenario.JsonAsync(await client.PostAsync(
            $"/api/storefront/drop/events/{item.EventId}/register", null));
        var repeated = await CommercialScenario.JsonAsync(await client.PostAsync(
            $"/api/storefront/drop/events/{item.EventId}/register", null));
        Assert.Equal(registration.GetProperty("id").GetInt64(), repeated.GetProperty("id").GetInt64());
        Assert.False(registration.GetProperty("isEligible").GetBoolean());

        var registrations = await CommercialScenario.JsonAsync(await client.GetAsync("/api/storefront/drop/registrations"));
        Assert.Single(registrations.EnumerateArray());
        var anotherBuyer = await scenario.Store.CreateCustomerAsync(scenario.Owner.EnterpriseId);
        using var other = scenario.Client(anotherBuyer.Token, anotherBuyer.Address);
        var privateRegistrations = await CommercialScenario.JsonAsync(await other.GetAsync("/api/storefront/drop/registrations"));
        Assert.Empty(privateRegistrations.EnumerateArray());
        var otherStore = await scenario.Store.CreateStoreAsync(planId: 3);
        var otherCustomer = await scenario.Store.CreateCustomerAsync(otherStore.EnterpriseId);
        using var otherTenant = scenario.Client(otherCustomer.Token, otherCustomer.Address);
        using var denied = await otherTenant.PostAsync($"/api/storefront/drop/events/{item.EventId}/register", null);
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
    }

}
