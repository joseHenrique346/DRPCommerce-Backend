namespace SharedCommerce.Application.Interfaces;

/// <summary>
/// Define a identificação confiável da loja antes do uso dos seus dados.
/// </summary>
public interface IRequestTenantResolver
{
    /// <summary>
    /// Define como identificar a loja da requisição para manter os dados separados entre tenants.
    /// </summary>
    Task ResolveAsync(Microsoft.AspNetCore.Http.HttpContext context);
}
