using System.Globalization;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Application.Security;

/// <summary>
/// Padroniza domínios para identificar lojas sem ambiguidades ou formatos indevidos.
/// </summary>
public static class HostName
{
    /// <summary>
    /// Padroniza e valida o domínio para comparar lojas sem aceitar protocolo, porta ou caminho indevidos.
    /// </summary>
    public static string Normalize(string host)
    {
        var value = host.Trim().TrimEnd('.');

        if (value.Contains('/') || value.Contains(':') || value.Contains('@') || value.Length > 253)
            throw new SecurityFailure(400, "Domain.InvalidHost", "Informe um domínio sem protocolo, porta ou caminho.");

        try
        {
            value = new IdnMapping().GetAscii(value).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            throw new SecurityFailure(400, "Domain.InvalidHost", "Domínio inválido.");
        }

        if (Uri.CheckHostName(value) != UriHostNameType.Dns || value.Split('.').Any(label => label.Length is 0 or > 63 || label.StartsWith('-') || label.EndsWith('-')))
            throw new SecurityFailure(400, "Domain.InvalidHost", "Domínio inválido.");

        return value;
    }
}
