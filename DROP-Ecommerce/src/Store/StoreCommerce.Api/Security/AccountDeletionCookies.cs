namespace StoreCommerce.Api.Security;

public static class AccountDeletionCookies
{
    public static void ClearDeletedAccountCookie(this HttpResponse response, bool customer)
    {
        var name = customer ? "__Host-DRP-StorefrontRefresh" : "__Host-DRP-BackofficeRefresh";
        response.Cookies.Delete(name, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }
}
