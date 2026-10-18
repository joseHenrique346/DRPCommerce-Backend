namespace DropCommerce.Application.Security;

// Only trusted customer workflows can open the write scope.
/// <summary>
/// Define o escopo confiável de gravação dos fluxos de cliente para impedir alterações fora das operações permitidas.
/// </summary>
public interface IDropOperationScope
{
    bool IsOpen { get; }
    /// <summary>
    /// Abre um escopo de gravação confiável para permitir somente alterações previstas nos fluxos de cliente.
    /// </summary>
    IDisposable Begin();
}
