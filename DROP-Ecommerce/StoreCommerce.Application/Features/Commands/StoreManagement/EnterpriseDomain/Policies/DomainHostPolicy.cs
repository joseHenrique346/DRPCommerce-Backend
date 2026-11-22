using SharedCommerce.Application.Security;

namespace StoreCommerce.Application.Contracts;

public static class DomainHostPolicy
{
    public static bool IsPlatformHost(string host, string baseDomain)
    {
        var normalized = HostName.Normalize(host);
        var platform = HostName.Normalize(baseDomain);

        return normalized != platform && normalized.EndsWith("." + platform, StringComparison.Ordinal);
    }
}
