using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Media;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;
using static StoreCommerce.Contracts.Tests.AppearanceContractsApiTests;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class MediaContractsApiTests(ContractsApiFixture fixture)
{
    internal static byte[] Logo => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAC0lEQVR4nGP4DwQACfsD/fteaysAAAAASUVORK5CYII=");

    [PostgresFact]
    public async Task Free_cannot_upload_logo_and_paid_upload_is_deduplicated_by_validated_content()
    {
        var free = await fixture.CreateStoreAsync();
        using var deniedClient = fixture.Client(free.Token);
        using var denied = await UploadAsync(deniedClient, Logo);
        Assert.Equal(HttpStatusCode.Conflict, denied.StatusCode);
        var paid = await fixture.CreateStoreAsync(planId: 4);
        using var client = fixture.Client(paid.Token);
        using var first = await UploadAsync(client, Logo);
        await EnsureAsync(first);
        using var firstJson = await JsonAsync(first);
        using var second = await UploadAsync(client, Logo);
        await EnsureAsync(second);
        using var secondJson = await JsonAsync(second);
        Assert.Equal(firstJson.RootElement.GetProperty("id").GetInt64(), secondJson.RootElement.GetProperty("id").GetInt64());
        Assert.Equal(1, firstJson.RootElement.GetProperty("width").GetInt32());
        Assert.Equal(1, firstJson.RootElement.GetProperty("height").GetInt32());
    }

    [PostgresFact]
    public async Task Upload_rejects_svg_fake_png_crc_corruption_and_size_over_limit()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        using var client = fixture.Client(owner.Token);
        using var svg = await UploadAsync(client, Encoding.UTF8.GetBytes("<svg xmlns='http://www.w3.org/2000/svg'></svg>"));
        Assert.Equal(HttpStatusCode.BadRequest, svg.StatusCode);
        var broken = Logo;
        broken[45] ^= 1;
        using var corrupted = await UploadAsync(client, broken);
        Assert.Equal(HttpStatusCode.BadRequest, corrupted.StatusCode);
        using var oversized = await UploadAsync(client, new byte[PngLogoValidator.MaximumBytes + 1]);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversized.StatusCode);
    }

    [PostgresFact]
    public async Task Initial_can_upload_preview_and_publish_logo_but_free_projection_removes_it()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync(planId: 4);
        var buyer = await fixture.CreateCustomerAsync(owner.EnterpriseId);
        using var client = fixture.Client(owner.Token);
        using var uploaded = await UploadAsync(client, Logo);
        await EnsureAsync(uploaded);
        using var asset = await JsonAsync(uploaded);
        var assetId = asset.RootElement.GetProperty("id").GetInt64();
        var url = asset.RootElement.GetProperty("url").GetString()!;
        var session = await BeginAsync(client, 1);
        session = await SaveAsync(client, session, AppearanceTestCatalog.Settings(logo: assetId));
        using var preview = await SendAsync(client, HttpMethod.Post, $"/api/appearances/edit-sessions/{session.Id}/preview", session.Version);
        await EnsureAsync(preview);
        using var previewJson = await JsonAsync(preview);
        Assert.Equal(url, previewJson.RootElement.GetProperty("manifest").GetProperty("assetBindings")
            .GetProperty("primaryLogo").GetProperty("url").GetString());
        session = new Edit(session.Id, previewJson.RootElement.GetProperty("session").GetProperty("version").GetInt64());
        using var published = await SendAsync(client, HttpMethod.Post, PublishPath(session.Id), session.Version);
        await EnsureAsync(published);
        using var storefront = fixture.Client();
        storefront.BaseAddress = buyer.Address;
        using var image = await storefront.GetAsync(url);
        image.EnsureSuccessStatusCode();
        Assert.Equal("image/png", image.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Logo, await image.Content.ReadAsByteArrayAsync());
        Assert.Contains("nosniff", image.Headers.GetValues("X-Content-Type-Options"));
        await fixture.ChangeContractAsync(owner.EnterpriseId, 1, null);
        using var hiddenImage = await storefront.GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, hiddenImage.StatusCode);
        using var manifest = await storefront.GetAsync("appearance");
        manifest.EnsureSuccessStatusCode();
        using var publicJson = await JsonAsync(manifest);
        Assert.Equal(JsonValueKind.Null, publicJson.RootElement.GetProperty("settings").GetProperty("brand").GetProperty("logoAssetId").ValueKind);
    }

    [PostgresFact]
    public async Task Logo_references_are_tenant_scoped_and_cannot_be_published_by_another_store()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var first = await fixture.CreateStoreAsync(planId: 4);
        var second = await fixture.CreateStoreAsync(planId: 4);
        using var client = fixture.Client(first.Token);
        using var upload = await UploadAsync(client, Logo);
        await EnsureAsync(upload);
        using var asset = await JsonAsync(upload);
        var id = asset.RootElement.GetProperty("id").GetInt64();
        using var other = fixture.Client(second.Token);
        using var hidden = await other.GetAsync($"/api/media-assets/{id}/content");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        var session = await BeginAsync(other, 1);
        session = await SaveAsync(other, session, AppearanceTestCatalog.Settings(logo: id));
        using var published = await SendAsync(other, HttpMethod.Post, PublishPath(session.Id), session.Version);
        Assert.Equal(HttpStatusCode.BadRequest, published.StatusCode);
    }

    [PostgresFact]
    public async Task Referenced_logo_is_preserved_and_unused_upload_can_be_deleted_and_binary_cleared()
    {
        await AppearanceTestCatalog.RegisterAsync(fixture);
        var owner = await fixture.CreateStoreAsync(planId: 4);
        using var client = fixture.Client(owner.Token);
        using var upload = await UploadAsync(client, Logo);
        await EnsureAsync(upload);
        using var asset = await JsonAsync(upload);
        var id = asset.RootElement.GetProperty("id").GetInt64();
        var session = await BeginAsync(client, 1);
        session = await SaveAsync(client, session, AppearanceTestCatalog.Settings(logo: id));
        using var used = await client.DeleteAsync("/api/media-assets/" + id);
        Assert.Equal(HttpStatusCode.Conflict, used.StatusCode);
        using var cancel = await SendAsync(client, HttpMethod.Post, $"/api/appearances/edit-sessions/{session.Id}/cancel", session.Version);
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
        using var deleted = await client.DeleteAsync("/api/media-assets/" + id);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var saved = await db.MediaAssets.IgnoreQueryFilters().SingleAsync(Asset => Asset.Id == id);
        Assert.True(saved.IsDeleted);
        Assert.Empty(saved.Content);
    }

    [PostgresFact]
    public async Task Identity_scope_alone_cannot_insert_media_outside_upload_workflow()
    {
        var owner = await fixture.CreateStoreAsync(planId: 4);
        using var scope = fixture.Services.CreateScope();
        using var identity = scope.ServiceProvider.GetRequiredService<StoreCommerce.Application.Interfaces.Identity.IIdentityOperationScope>().Begin();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var image = PngLogoValidator.Validate(Logo);
        db.MediaAssets.Add(StoreCommerce.Domain.Entity.MediaAsset.Create(owner.EnterpriseId, image.Content, image.Hash, image.Width, image.Height));
        var denied = await Assert.ThrowsAsync<SharedCommerce.Domain.Exceptions.SecurityFailure>(() => db.SaveChangesAsync());
        Assert.Equal("Media.UseWorkflow", denied.Code);
    }

    internal static async Task<HttpResponseMessage> UploadAsync(HttpClient client, byte[] content)
    {
        using var body = new MultipartFormDataContent();
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        body.Add(file, "file", "logo.png");

        return await client.PostAsync("/api/media-assets", body);
    }
}

public sealed class PngLogoTests
{
    [Fact]
    public void Accepts_a_complete_static_png_and_rejects_truncation_or_trailing_payload()
    {
        var image = MediaContractsApiTests.Logo;
        var validated = PngLogoValidator.Validate(image);
        Assert.Equal(image, validated.Content);
        Assert.Equal(1, validated.Width);
        Assert.Throws<SharedCommerce.Domain.Exceptions.SecurityFailure>(() => PngLogoValidator.Validate(image[..^4]));
        Assert.Throws<SharedCommerce.Domain.Exceptions.SecurityFailure>(() => PngLogoValidator.Validate([.. image, 1]));
    }

    [Fact]
    public void Removes_optional_text_metadata_and_keeps_the_canonical_image()
    {
        var withMetadata = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAGHRFWHRDb21tZW50AHByaXZhdGUgbWV0YWRhdGGezJzmAAAAC0lEQVR4nGP4DwQACfsD/fteaysAAAAASUVORK5CYII=");
        var cleaned = PngLogoValidator.Validate(withMetadata);
        Assert.Equal(MediaContractsApiTests.Logo, cleaned.Content);
        Assert.Equal(PngLogoValidator.Validate(MediaContractsApiTests.Logo).Hash, cleaned.Hash);
    }
}
