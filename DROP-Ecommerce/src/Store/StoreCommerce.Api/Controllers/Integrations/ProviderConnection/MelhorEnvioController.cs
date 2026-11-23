using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route(MelhorEnvioOptions.Route)]
public sealed class MelhorEnvioController(IMediator mediator, IOptions<MelhorEnvioOptions> options) : ControllerBase
{
    [HttpPost("oauth/start")]
    public async Task<IActionResult> StartAsync([FromBody] BeginMelhorEnvioAuthorizationCommand command, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await mediator.Send(command, ct);
        var cookie = BindingOptions();
        cookie.Expires = new DateTimeOffset(result.ExpiresAt);
        Response.Cookies.Append(BindingCookie(command.Environment), result.BrowserBinding, cookie);

        return Ok(new { result.AuthorizationUrl, result.ExpiresAt });
    }

    [AllowAnonymous]
    [HttpGet("oauth/{environment}/callback")]
    public async Task<IActionResult> CallbackAsync([FromRoute] IntegrationEnvironment environment, [FromQuery] string? state,
        [FromQuery] string? code, [FromQuery] string? error, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        var cookie = BindingCookie(environment);
        var binding = Request.Cookies[cookie] ?? string.Empty;
        Response.Cookies.Delete(cookie, BindingOptions());
        await mediator.Send(new CompleteMelhorEnvioAuthorizationCommand(environment, state ?? string.Empty, binding, code, error), ct);

        return Redirect(QueryHelpers.AddQueryString(options.Value.BackofficeReturnUrl, "melhorEnvio", "connected"));
    }

    private static string BindingCookie(IntegrationEnvironment environment)
    {
        return "__Host-me-oauth-" + environment.ToString().ToLowerInvariant();
    }

    private static CookieOptions BindingOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            IsEssential = true
        };
    }
}
