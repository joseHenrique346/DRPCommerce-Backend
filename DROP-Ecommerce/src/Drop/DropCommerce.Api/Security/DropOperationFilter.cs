using Commerce.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DropCommerce.Api.Security;

/// <summary>
/// Protege os endpoints administrativos do Drop conforme as permissões e o tipo de operação.
/// </summary>
public sealed class DropOperationFilter : IAsyncAuthorizationFilter
{
    /// <summary>
    /// Exige as permissões das operações administrativas do Drop e impede alterações manuais nos logs de auditoria.
    /// </summary>
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            return Task.CompletedTask;

        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor ||
            descriptor.ControllerName is
                "StorefrontDropEvent" or
                "StorefrontDropRegistration" or
                "StorefrontQueueEntry" or
                "StorefrontDropReservation" or
                "StorefrontDropOrder" or
                "StorefrontWaitlistEntry" or
                "StorefrontDropNotification")
            return Task.CompletedTask;

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true || user.FindFirst(SecurityNames.Actor)?.Value != "user" || user.FindFirst(SecurityNames.Membership) is null)
        {
            context.Result = new UnauthorizedResult();

            return Task.CompletedTask;
        }

        var read = descriptor.ActionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase);

        if (descriptor.ControllerName == "DropAuditLog" && !read)
        {
            context.Result = new ForbidResult();

            return Task.CompletedTask;
        }

        var permission = descriptor.ControllerName switch
        {
            "DropEvent" or "DropProduct" or "DropCoupon" => read ? "drop.events.view" : "drop.events.manage",
            "DropOrder" or "DropOrderItem" or "DropTransaction" => read ? "orders.view" : "orders.manage",
            "DropRegistration" or "QueueEntry" or "QueueSession" or "WaitlistEntry" or "DropReservation" or "DropNotification" => "drop.events.manage",
            _ => null
        };

        if (permission is null ? user.FindFirst(SecurityNames.Kind)?.Value != "primary_owner" : !SecurityNames.HasPermission(user, permission))
            context.Result = new ObjectResult(new ProblemDetails { Status = 403, Title = "Acesso não permitido." }) { StatusCode = 403 };

        return Task.CompletedTask;
    }
}
