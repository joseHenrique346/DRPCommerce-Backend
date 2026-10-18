using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StoreCommerce.Api.Security;

/// <summary>
/// Protege operações de autenticação contra requisições forjadas que reutilizem cookies do navegador.
/// </summary>
public sealed class CookieCsrfAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Valida o token CSRF nas operações de escrita para impedir ações forjadas usando cookies do navegador.
    /// </summary>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (HttpMethods.IsGet(context.HttpContext.Request.Method) || HttpMethods.IsHead(context.HttpContext.Request.Method))
            return;

        var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException exception)
        {
            context.HttpContext.RequestServices.GetRequiredService<ILogger<CookieCsrfAttribute>>()
                .LogDebug("Validação CSRF rejeitada: {Reason}", exception.Message);
            context.Result = new ObjectResult(new ProblemDetails { Status = 403, Title = "Token CSRF inválido." }) { StatusCode = 403 };
        }
    }
}

