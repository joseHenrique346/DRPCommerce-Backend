using System.Net;
using System.Text.Json;
using DropCommerce.Application.Appearance;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Features.Commands;
using SharedCommerce.Domain.Exceptions;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class DropAppearanceContractsTests(ContractsApiFixture fixture)
{
    [PostgresFact]
    public async Task Drop_uses_ten_models_even_when_commerce_has_twenty_store_models_and_rechecks_module_loss()
    {
        var owner = await fixture.CreateStoreAsync(planId: 3);
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(fixture, owner);
        var catalog = await drop.Mediator.Send(new GetAppearanceThemesQuery());
        Assert.Equal(10, catalog.Select(Theme => Theme.Key).Distinct().Count());
        var blockedTheme = await Assert.ThrowsAsync<AppearanceFailure>(() => drop.Mediator.Send(new BeginAppearanceEditCommand(
            DropAppearanceTargets.Default, new BeginAppearanceEditRequest("contract-11", "1.0.0"))));
        Assert.Equal("Appearance.ThemeNotAllowed", blockedTheme.Code);
        var session = await drop.Mediator.Send(new BeginAppearanceEditCommand(
            DropAppearanceTargets.Default, new BeginAppearanceEditRequest("contract-10", "1.0.0")));
        await drop.Mediator.Send(new PublishAppearanceCommand(DropAppearanceTargets.Default, session.SessionId, session.Version));
        await fixture.ChangeContractAsync(owner.EnterpriseId, 2, null);
        var publicFailure = await Assert.ThrowsAsync<AppearanceFailure>(() => drop.Mediator.Send(new GetPublishedAppearanceQuery(DropAppearanceTargets.Default)));
        Assert.Equal(404, publicFailure.Status);
        Assert.Empty(await drop.Mediator.Send(new GetAppearanceThemesQuery()));
        Assert.False(drop.Services.GetRequiredService<DropCommerceDbContext>().Database.HasPendingModelChanges());
    }

    [PostgresFact]
    public async Task Drop_logo_validation_calls_store_and_retention_protects_local_deletion()
    {
        var owner = await fixture.CreateStoreAsync(planId: 5);
        using var client = fixture.Client(owner.Token);
        using var uploaded = await MediaContractsApiTests.UploadAsync(client, MediaContractsApiTests.Logo);
        await AppearanceContractsApiTests.EnsureAsync(uploaded);
        using var json = await AppearanceContractsApiTests.JsonAsync(uploaded);
        var assetId = json.RootElement.GetProperty("id").GetInt64();
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(fixture, owner);
        var session = await drop.Mediator.Send(new BeginAppearanceEditCommand(
            DropAppearanceTargets.Default, new BeginAppearanceEditRequest("contract-01", "1.0.0")));
        session = await drop.Mediator.Send(new SaveAppearanceCommand(DropAppearanceTargets.Default, session.SessionId, session.Version,
            new SaveAppearanceRequest(JsonSerializer.SerializeToElement(AppearanceTestCatalog.Settings(logo: assetId)))));
        var manifest = await drop.Mediator.Send(new PublishAppearanceCommand(DropAppearanceTargets.Default, session.SessionId, session.Version));
        Assert.Equal(assetId, manifest.AssetBindings["primaryLogo"].GetProperty("assetId").GetInt64());
        using var deleted = await client.DeleteAsync("/api/media-assets/" + assetId);
        Assert.Equal(HttpStatusCode.Conflict, deleted.StatusCode);
        var publicManifest = await drop.Mediator.Send(new GetPublishedAppearanceQuery(DropAppearanceTargets.Default));
        Assert.Equal(manifest.Theme.Key, publicManifest.Theme.Key);
    }
}
