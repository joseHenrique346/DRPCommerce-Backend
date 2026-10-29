using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Api.Security;

// Deny by default. Unmapped commercial modules remain principal-owner-only until the permission matrix is expanded.
/// <summary>
/// Protege operações comerciais do Store e nega permissões ainda não mapeadas para usuários comuns.
/// </summary>
public sealed class StoreOperationFilter : IAsyncAuthorizationFilter
{
    /// <summary>
    /// Aplica a matriz de permissões e restringe módulos não mapeados ao dono principal para negar acessos por padrão.
    /// </summary>
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            return Task.CompletedTask;

        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return Task.CompletedTask;

        var controller = descriptor.ControllerName;

        if (controller is
            "BackofficeAuth" or
            "StorefrontAuth" or
            "User" or
            "Enterprise" or
            "EnterpriseMembership" or
            "AccessRole" or
            "AccessRolePermission" or
            "MembershipAccessRole" or
            "EnterpriseDomain" or
            "Invitation" or
            "InvitationAcceptance" or
            "OwnershipTransfer" or
            "OwnershipTransferAcceptance" or
            "StorefrontCustomer" or
            "StorefrontEnterprise" or
            "StorefrontOrder" or
            "StorefrontProduct" or
            "InternalIdentity" or
            "InternalTenant" or
            "InternalCustomer" or
            "InternalEmployee" or
            "InternalProduct" or
            "InternalEnterprise" or
            "Permission" or
            "EnterpriseMembershipKind")
            return Task.CompletedTask;

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true || user.FindFirst(SecurityNames.Actor)?.Value != "user" ||
            user.FindFirst(SecurityNames.Membership) is null)
        {
            context.Result = new UnauthorizedResult();

            return Task.CompletedTask;
        }

        var action = descriptor.ActionName;
        var read = action.StartsWith("Get", StringComparison.OrdinalIgnoreCase);
        var verb = context.HttpContext.Request.Method;
        string? permission = controller switch
        {
            "Product" or "ProductVariant" when action.StartsWith("UpdatePricing", StringComparison.OrdinalIgnoreCase) => PermissionCodes.ProductsChangePrice,
            "ProductVariant" => read ? PermissionCodes.ProductsView : verb == "POST" ? PermissionCodes.ProductsCreate :
                verb == "DELETE" ? PermissionCodes.ProductsDelete : PermissionCodes.ProductsEdit,
            "Product" => read ? PermissionCodes.ProductsView :
                verb == "POST" ? PermissionCodes.ProductsCreate :
                verb == "DELETE" ? PermissionCodes.ProductsDelete : PermissionCodes.ProductsEdit,
            "Category" => read ? PermissionCodes.CategoriesView : PermissionCodes.CategoriesManage,
            "Coupon" => read ? PermissionCodes.CouponsView : PermissionCodes.CouponsManage,
            "Supplier" => PermissionCodes.SuppliersManage,
            "Inventory" or "StockMovement" => PermissionCodes.InventoryManage,
            "Order" when action.StartsWith("Cancel", StringComparison.OrdinalIgnoreCase) => PermissionCodes.OrdersCancel,
            "Order" when action.StartsWith("Refund", StringComparison.OrdinalIgnoreCase) => PermissionCodes.OrdersRefund,
            "Order" or "OrderItem" => read ? PermissionCodes.OrdersView : PermissionCodes.OrdersManage,
            "Customer" => read ? PermissionCodes.CustomersView : PermissionCodes.CustomersManage,
            "Employee" or "EmployeePosition" or "Department" => PermissionCodes.EmployeesManage,
            "StoreSettings" => PermissionCodes.SettingsManage,
            _ => null
        };
        var allowed = permission is null ? user.FindFirst(SecurityNames.Kind)?.Value == "primary_owner" : SecurityNames.HasPermission(user, permission);

        if (!allowed)
            context.Result = new ObjectResult(new ProblemDetails { Status = 403, Title = "Acesso não permitido." }) { StatusCode = 403 };

        return Task.CompletedTask;
    }
}
