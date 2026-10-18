using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using StoreCommerce.Api.Security;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StoreCommerce.Api.Configuration.Swagger;

/// <summary>
/// Documenta o header exigido pelas rotas com proteção CSRF para viabilizar seu uso no Swagger.
/// </summary>
public sealed class CsrfHeaderOperationFilter : IOperationFilter
{
    private const string HeaderName = "X-CSRF-TOKEN";

    /// <summary>
    /// Exibe o header CSRF no Swagger apenas onde ele é exigido para permitir testes sem remover a proteção.
    /// </summary>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var httpMethod = context.ApiDescription.HttpMethod;

        if (httpMethod is null || HttpMethods.IsGet(httpMethod) || HttpMethods.IsHead(httpMethod))
            return;

        var requiresCsrf = context.ApiDescription.ActionDescriptor.FilterDescriptors
            .Any(filterDescriptor => filterDescriptor.Filter is CookieCsrfAttribute);

        if (!requiresCsrf)
            return;

        operation.Parameters ??= new List<OpenApiParameter>();

        if (operation.Parameters.Any(parameter => parameter.In == ParameterLocation.Header &&
            string.Equals(parameter.Name, HeaderName, StringComparison.OrdinalIgnoreCase)))
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = HeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "requestToken retornado pelo GET csrf. Mantenha o cookie e o mesmo contexto de autenticação.",
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}
