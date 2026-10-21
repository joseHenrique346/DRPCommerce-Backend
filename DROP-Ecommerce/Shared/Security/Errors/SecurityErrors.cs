using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Commerce.Security;

/// <summary>
/// Padroniza falhas esperadas para responder sem divulgar detalhes internos da aplicação.
/// </summary>
public static class SecurityErrors
{
    /// <summary>
    /// Converte falhas conhecidas em respostas seguras, sem expor detalhes internos do banco ou da aplicação.
    /// </summary>
    public static SecurityFailure? Classify(Exception exception) => exception switch
    {
        SecurityFailure failure => failure,
        Commerce.Appearance.Domain.AppearanceFailure failure => new(failure.Status, failure.Code, failure.Message),
        ArgumentException => new(400, "Validation.InvalidInput", "Os dados informados são inválidos."),
        DbUpdateConcurrencyException => new(409, "Data.ConcurrentChange", "O registro foi alterado. Atualize e tente novamente."),
        DbUpdateException { InnerException: PostgresException { SqlState: "23505" } } => new(409, "Data.Duplicate", "Já existe um registro com estes dados."),
        DbUpdateException { InnerException: PostgresException { SqlState: "23503" } } => new(400, "Data.InvalidRelationship", "Referência inválida ou registro em uso."),
        _ => null
    };
}
