using MediatR;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Integrations.MercadoPago;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/integrations/mercado-pago")]
public sealed class MercadoPagoController(IMediator mediator, IOptions<MercadoPagoOptions> options) : ControllerBase
{
    private const string BindingCookie = "__Host-mp-oauth";

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";

        return Ok(await mediator.Send(new GetMercadoPagoIntegrationQuery(), ct));
    }

    [HttpPost("oauth/start")]
    public async Task<IActionResult> StartAsync([FromBody] MercadoPagoConnectRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new BeginMercadoPagoAuthorizationCommand(request.Environment), ct);
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
        Response.Headers.CacheControl = "no-store";

        return Ok(await mediator.Send(new ListPendingProviderNotificationsQuery(), ct));
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
        await mediator.Send(new CompleteMercadoPagoAuthorizationCommand(state ?? string.Empty, binding, code, error), ct);

        return Redirect(QueryHelpers.AddQueryString(options.Value.BackofficeReturnUrl, "mercadoPago", "connected"));
    }

    /// <summary>
    /// Conecta exclusivamente o vendedor de teste configurado no armazenamento de segredos do servidor.
    /// </summary>
    [HttpPost("sandbox/connect")]
    public async Task<IActionResult> ConnectSandboxAsync(CancellationToken ct)
    {
        await mediator.Send(new ConnectMercadoPagoSandboxCommand(), ct);

        return NoContent();
    }

    [HttpPut("selection")]
    public async Task<IActionResult> SelectAsync([FromBody] PaymentProviderSelectionRequest request, CancellationToken ct)
    {
        await mediator.Send(new SelectPaymentProviderCommand(request.Provider, request.Environment), ct);

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

        await mediator.Send(new ReceiveMercadoPagoNotificationCommand(connectionId, id, Request.Headers["x-request-id"].ToString(),
            Request.Headers["x-signature"].ToString()), ct);

        return Ok();
    }
}
