using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Result;
using Commerce.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Security;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Api.Controllers.Base;

/// <summary>
/// Centraliza cookies e CSRF da autenticação para manter a proteção igual nas rotas administrativas e de cliente.
/// </summary>
[ApiController]
[CookieCsrf]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public abstract class AuthControllerBase(IMediator mediator, IAntiforgery antiforgery) : ControllerBase
{
    protected IMediator Mediator => mediator;
    protected abstract bool CustomerContext
    {
        get;
    }
    private string CookieName => CustomerContext ? "__Host-DRP-StorefrontRefresh" : "__Host-DRP-BackofficeRefresh";

    /// <summary>
    /// Gera o token CSRF e seu cookie para proteger as próximas operações de autenticação.
    /// </summary>
    protected IActionResult Csrf()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            requestToken = tokens.RequestToken,
            headerName = "X-CSRF-TOKEN"
        });
    }

    /// <summary>
    /// Grava o refresh em cookie protegido somente após sucesso e retorna o access token sem expor o refresh no JSON.
    /// </summary>
    protected IActionResult Session(Result<SessionResult> result)
    {
        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        var session = result.Content!;

        Response.Cookies.Append(CookieName, session.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
            Expires = new DateTimeOffset(session.RefreshTokenExpiresAt, TimeSpan.Zero)
        });

        return Ok(session);
    }

    /// <summary>
    /// Obtém o refresh do cookie protegido e rejeita a renovação quando não existe uma sessão no navegador.
    /// </summary>
    protected string RefreshCookie()
    {
        return Request.Cookies[CookieName] ??
            throw new SecurityFailure(401, "Identity.RefreshRequired", "Sessão de renovação ausente.");
    }

    /// <summary>
    /// Remove o cookie de refresh para que o navegador deixe de usar a sessão encerrada.
    /// </summary>
    protected void ClearCookie()
    {
        Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }
}

