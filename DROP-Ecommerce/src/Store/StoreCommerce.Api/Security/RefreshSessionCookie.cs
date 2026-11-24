using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using System.Globalization;

namespace StoreCommerce.Api.Security;

public static class RefreshSessionCookie
{
    public static string Name(HttpContext context, bool customer)
    {
        if (!customer)
            return "__Host-DRP-BackofficeRefresh";

        if (context.Items[SecurityNames.ResolvedTenant] is not long enterpriseId || enterpriseId <= 0)
            throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        return "__Host-DRP-StorefrontRefresh-" + enterpriseId.ToString(CultureInfo.InvariantCulture);
    }

    public static CookieOptions Options(DateTimeOffset? expires = null)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
            Expires = expires
        };
    }

    public static void ClearRefreshCookie(this HttpResponse response, bool customer)
    {
        response.Cookies.Delete(Name(response.HttpContext, customer), Options());
    }
}
