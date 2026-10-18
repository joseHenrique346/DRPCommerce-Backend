using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Commerce.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers;

[Route("api/auth")]
[Authorize(Policy = SecurityNames.GlobalUser)]
[EnableRateLimiting("auth")]
public sealed class BackofficeAuthController(IMediator mediator, IAntiforgery antiforgery) : AuthControllerBase(mediator, antiforgery)
{
    protected override bool CustomerContext => false;

    /// <summary>
    /// Obtém o token CSRF para as operações de autenticação.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("csrf")]
    public IActionResult GetCsrf()
    {
        return Csrf();
    }

    /// <summary>
    /// Cadastra uma conta administrativa.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await Mediator.Send(new RegisterUserCommand(request), ct));
    }

    /// <summary>
    /// Autentica o usuário e inicia uma sessão.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        return Session(await Mediator.Send(new LoginUserCommand(request), ct));
    }

    /// <summary>
    /// Renova o access token e rotaciona o refresh token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [EnableRateLimiting("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        return Session(await Mediator.Send(new RefreshCommand(RefreshCookie(), false, null), ct));
    }

    /// <summary>
    /// Seleciona uma loja e renova os tokens da sessão.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("select-enterprise")]
    [EnableRateLimiting("refresh")]
    public async Task<IActionResult> Select(SelectEnterpriseRequest request, CancellationToken ct)
    {
        return Session(await Mediator.Send(new RefreshCommand(RefreshCookie(), false, request.MembershipId), ct));
    }

    /// <summary>
    /// Encerra a sessão atual e remove o cookie de refresh.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.ContainsKey(CustomerContext ? "__Host-DRP-StorefrontRefresh" : "__Host-DRP-BackofficeRefresh"))
        {
            var result = await Mediator.Send(new LogoutCommand(RefreshCookie(), CustomerContext), ct);

            if (result.IsFailure)
                return this.ToIdentityActionResult(result);
        }

        ClearCookie();

        return NoContent();
    }

    /// <summary>
    /// Encerra todas as sessões do usuário.
    /// </summary>
    [Authorize(Policy = SecurityNames.GlobalUser)]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new LogoutAllCommand(false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        ClearCookie();

        return NoContent();
    }

    /// <summary>
    /// Lista as sessões do usuário autenticado.
    /// </summary>
    [Authorize(Policy = SecurityNames.GlobalUser)]
    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await Mediator.Send(new ListSessionsQuery(false), ct));
    }

    /// <summary>
    /// Revoga a sessão informada.
    /// </summary>
    [Authorize(Policy = SecurityNames.GlobalUser)]
    [HttpDelete("sessions/{familyId:guid}")]
    public async Task<IActionResult> Revoke(Guid familyId, CancellationToken ct)
    {
        var result = await Mediator.Send(new RevokeSessionCommand(familyId, false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }

    /// <summary>
    /// Altera a senha do usuário e invalida suas sessões.
    /// </summary>
    [Authorize(Policy = SecurityNames.GlobalUser)]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new ChangePasswordCommand(request, false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        ClearCookie();

        return NoContent();
    }

    /// <summary>
    /// Solicita recuperação de senha; indisponível nesta etapa.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestReset(AccountActionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RequestActionCommand(request, AccountActionPurpose.ResetPassword, false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return Accepted();
    }

    /// <summary>
    /// Redefine a senha por token; indisponível nesta etapa.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(ConsumeActionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new ConsumeActionCommand(request, AccountActionPurpose.ResetPassword, false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        ClearCookie();

        return NoContent();
    }

    /// <summary>
    /// Solicita confirmação de e-mail; indisponível nesta etapa.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("request-email-confirmation")]
    public async Task<IActionResult> RequestConfirmation(AccountActionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RequestActionCommand(request, AccountActionPurpose.ConfirmEmail, false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return Accepted();
    }

    /// <summary>
    /// Confirma o e-mail por token; indisponível nesta etapa.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> Confirm(ConsumeActionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new ConsumeActionCommand(request, AccountActionPurpose.ConfirmEmail, false), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        return NoContent();
    }
}

