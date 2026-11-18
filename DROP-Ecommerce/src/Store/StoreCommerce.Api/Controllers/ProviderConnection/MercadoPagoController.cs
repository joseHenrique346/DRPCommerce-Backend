using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

namespace StoreCommerce.Api.Controllers;

public sealed record MercadoPagoConnectRequest(IntegrationEnvironment Environment);
public sealed record PaymentProviderSelectionRequest(string Provider, IntegrationEnvironment Environment);
public sealed record MercadoPagoWebhookData(JsonElement Id);
public sealed record MercadoPagoWebhookBody(string? Type, MercadoPagoWebhookData? Data);

[ApiController]
[Route("api/integrations/mercado-pago")]
public sealed class MercadoPagoController(MercadoPagoAuthorizationService authorization, MercadoPagoNotifications notifications,
    IOptions<MercadoPagoOptions> options, ICurrentIdentity actor, IProviderConnectionStore connections,
    IPaymentGatewayRegistry gateways, StoreCommerceDbContext context) : ControllerBase
{
    private const string BindingCookie = "__Host-mp-oauth";

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        Response.Headers.CacheControl = "no-store";
        var selected = await context.Set<PaymentProviderSelection>().IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Selection => Selection.EnterpriseId == actor.EnterpriseId, ct);
        var available = (await connections.ListAsync(actor.EnterpriseId, ct))
            .Where(Connection => Connection.Provider == IntegrationProviders.MercadoPago)
            .Select(Connection => new
            {
                Connection.Id, Connection.Environment, Connection.ExternalAccountId, Connection.IsConnected,
                Connection.AccessTokenExpiresAt, Connection.ConnectedAt, Connection.DisconnectedAt,
                webhookUrl = options.Value.WebhookUrl(Connection.Id)
            }).ToList();

        return Ok(new { enabled = options.Value.Enabled, productionAllowed = options.Value.AllowProduction,
            applicationId = options.Value.ClientId, selectedProvider = selected?.Provider ?? IntegrationProviders.Manual,
            selectedEnvironment = selected?.Environment, connections = available });
    }

    [HttpPost("oauth/start")]
    public async Task<IActionResult> StartAsync([FromBody] MercadoPagoConnectRequest request, CancellationToken ct)
    {
        var result = await authorization.BeginAsync(request.Environment, ct);
        Response.Headers.CacheControl = "no-store";
        Response.Cookies.Append(BindingCookie, result.BrowserBinding, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(10),
            IsEssential = true
        });

        return Ok(new { result.AuthorizationUrl, result.ExpiresAt });
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotificationsAsync(CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        Response.Headers.CacheControl = "no-store";
        var notifications = await context.Set<ProviderNotification>().IgnoreQueryFilters().AsNoTracking()
            .Where(Notification => Notification.EnterpriseId == actor.EnterpriseId && Notification.Status != ProviderNotificationStatus.Completed)
            .OrderByDescending(Notification => Notification.Id).Take(100)
            .Select(Notification => new { Notification.Id, Notification.ProviderConnectionId, Notification.PaymentAttemptId,
                Notification.PaymentId, Notification.Status, Notification.ResolutionCode, Notification.CreatedAt }).ToListAsync(ct);

        return Ok(notifications);
    }

    [AllowAnonymous]
    [HttpGet("oauth/callback")]
    public async Task<IActionResult> CallbackAsync([FromQuery] string? state, [FromQuery] string? code,
        [FromQuery] string? error, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        var binding = Request.Cookies[BindingCookie] ?? string.Empty;
        Response.Cookies.Delete(BindingCookie, new CookieOptions { Secure = true, HttpOnly = true, SameSite = SameSiteMode.Lax, Path = "/" });
        await authorization.CompleteAsync(state ?? string.Empty, binding, code, error, ct);

        return Redirect(QueryHelpers.AddQueryString(options.Value.BackofficeReturnUrl, "mercadoPago", "connected"));
    }

    /// <summary>
    /// Conecta exclusivamente o vendedor de teste configurado no armazenamento de segredos do servidor.
    /// </summary>
    [HttpPost("sandbox/connect")]
    public async Task<IActionResult> ConnectSandboxAsync(CancellationToken ct)
    {
        await authorization.ConnectSandboxAsync(ct);

        return NoContent();
    }

    [HttpPut("selection")]
    public async Task<IActionResult> SelectAsync([FromBody] PaymentProviderSelectionRequest request, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        await gateways.SelectAsync(actor.EnterpriseId, request.Provider, request.Environment, ct);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("webhooks/{connectionId:long}")]
    [Consumes("application/json")]
    [RequestSizeLimit(16_384)]
    public async Task<IActionResult> WebhookAsync(long connectionId, [FromBody] MercadoPagoWebhookBody body, CancellationToken ct)
    {
        if (Request.Query["data.id"].Count != 1 || Request.Headers["x-request-id"].Count != 1 ||
            Request.Headers["x-signature"].Count != 1 || body.Data is null ||
            body.Data.Id.ValueKind is not (JsonValueKind.Number or JsonValueKind.String))
            throw new SecurityFailure(400, "MercadoPago.InvalidNotification", "Notificação incompleta.");

        var id = body.Data.Id.ValueKind == JsonValueKind.String ? body.Data.Id.GetString()! : body.Data.Id.GetRawText();

        if (id != Request.Query["data.id"].ToString())
            throw new SecurityFailure(400, "MercadoPago.NotificationIdMismatch", "O corpo e a assinatura identificam pagamentos diferentes.");

        if (body.Type != "payment")
            throw new SecurityFailure(400, "MercadoPago.InvalidTopic", "Este endpoint recebe somente notificações de pagamento.");

        await notifications.ReceiveAsync(connectionId, id, Request.Headers["x-request-id"].ToString(),
            Request.Headers["x-signature"].ToString(), ct);

        return Ok();
    }
}
