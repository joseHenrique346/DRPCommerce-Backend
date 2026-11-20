using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Infrastructure.Data.Billing;
using System.Text.Json;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprises/current/billing")]
[Authorize(Policy = SecurityNames.GlobalUser)]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
public sealed class PlatformBillingController(PlatformBillingService billing) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        return Ok(await billing.GetAsync(ct));
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> Invoices([FromQuery] long? beforeId, CancellationToken ct)
    {
        return Ok(await billing.ListInvoicesAsync(beforeId, ct));
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(StartPlatformBillingRequest request, CancellationToken ct)
    {
        return Accepted(await billing.StartAsync(request.ContractChangeId, ct));
    }

    [HttpPost("reconcile")]
    public async Task<IActionResult> Reconcile(CancellationToken ct)
    {
        return Ok(await billing.ReconcileCurrentAsync(ct));
    }

    [HttpPut("pending-team")]
    public async Task<IActionResult> SelectTeam(PlatformBillingTeamSelection request, CancellationToken ct)
    {
        await billing.SelectTeamAsync(request, ct);

        return NoContent();
    }
}

public sealed record PlatformWebhookData(JsonElement Id);
public sealed record PlatformWebhookBody(string? Type, PlatformWebhookData? Data);

[ApiController]
[Route("api/platform-billing/mercado-pago/webhook")]
public sealed class PlatformBillingWebhookController(PlatformBillingNotifications notifications) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [RequestSizeLimit(16_384)]
    public async Task<IActionResult> Receive(PlatformWebhookBody body, CancellationToken ct)
    {
        if (Request.Query["data.id"].Count != 1 || Request.Headers["x-request-id"].Count != 1 ||
            Request.Headers["x-signature"].Count != 1 || body.Type is null || body.Data is null ||
            body.Data.Id.ValueKind is not (JsonValueKind.Number or JsonValueKind.String))
            throw new SecurityFailure(400, "Billing.InvalidNotification", "Notificação financeira incompleta.");

        var id = body.Data.Id.ValueKind == JsonValueKind.String ? body.Data.Id.GetString()! : body.Data.Id.GetRawText();

        if (id != Request.Query["data.id"].ToString())
            throw new SecurityFailure(400, "Billing.NotificationIdMismatch", "O corpo e a assinatura identificam recursos diferentes.");

        await notifications.ReceiveAsync(body.Type, id, Request.Headers["x-request-id"].ToString(),
            Request.Headers["x-signature"].ToString(), ct);

        return Ok();
    }
}
