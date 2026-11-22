using MediatR;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using System.Text.Json;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/platform-billing/mercado-pago/webhook")]
public sealed class PlatformBillingWebhookController(IMediator mediator) : ControllerBase
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

        var result = await mediator.Send(new ReceivePlatformBillingNotificationCommand(
            body.Type, id, Request.Headers["x-request-id"].ToString(), Request.Headers["x-signature"].ToString()), ct);

        return result.IsSuccess ? Ok() : this.ToIdentityActionResult(result);
    }
}
