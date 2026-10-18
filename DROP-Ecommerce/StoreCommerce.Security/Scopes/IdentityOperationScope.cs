using StoreCommerce.Application.Identity;

namespace StoreCommerce.Security.Scopes;

// Only trusted application services open this scope; it is never bound from an HTTP request.
/// <summary>
/// Controla as gravações sensíveis de identidade sem permitir que a requisição abra esse acesso diretamente.
/// </summary>
public sealed class IdentityOperationScope : IIdentityOperationScope
{
    private int depth;
    public bool IsOpen => depth > 0;

    /// <summary>
    /// Abre um escopo temporário de identidade para permitir gravações sensíveis somente em fluxos confiáveis.
    /// </summary>
    public IDisposable Begin()
    {
        depth++;

        return new Lease(this);
    }

    /// <summary>
    /// Representa um nível aberto do escopo e garante seu fechamento ao concluir a operação.
    /// </summary>
    private sealed class Lease(IdentityOperationScope owner) : IDisposable
    {
        /// <summary>
        /// Fecha este nível do escopo para não manter gravações sensíveis liberadas após o término da operação.
        /// </summary>
        public void Dispose()
        {
            if (owner.depth > 0)
                owner.depth--;
        }
    }
}
