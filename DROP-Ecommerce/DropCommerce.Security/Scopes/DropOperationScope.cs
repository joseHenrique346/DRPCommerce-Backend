using DropCommerce.Application.Security;

namespace DropCommerce.Security.Scopes;

/// <summary>
/// Controla quando um fluxo de cliente pode gravar dados sem liberar alterações diretas de privilégios.
/// </summary>
public sealed class DropOperationScope : IDropOperationScope
{
    private int depth;
    public bool IsOpen => depth > 0;

    /// <summary>
    /// Abre um escopo temporário para autorizar gravações dos fluxos de cliente sem liberar o CRUD diretamente.
    /// </summary>
    public IDisposable Begin()
    {
        depth++;

        return new Lease(this);
    }

    /// <summary>
    /// Representa um nível aberto do escopo e garante seu fechamento ao concluir a operação.
    /// </summary>
    private sealed class Lease(DropOperationScope owner) : IDisposable
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
