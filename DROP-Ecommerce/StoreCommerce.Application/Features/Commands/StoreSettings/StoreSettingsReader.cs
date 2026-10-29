using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Fornece as configurações e a disponibilidade da loja atual para as regras de venda.
/// </summary>
public sealed class StoreSettingsReader(
    IRepository<StoreSettings> settingsRepository,
    IRepository<Enterprise> enterpriseRepository,
    ITenantProvider tenantProvider)
{
    public static Error InactiveStoreError { get; } =
        Error.Conflict("Store.Inactive", "A loja está inativa e não aceita novos pedidos nem alterações em pedidos pendentes.");

    /// <summary>
    /// Retorna as configurações persistidas da loja ou, se ainda não existirem, a configuração padrão (não persistida).
    /// </summary>
    public async Task<StoreSettings> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var enterpriseId = tenantProvider.GetEnterpriseId();

        return await settingsRepository.FindAsync(settings => settings.EnterpriseId == enterpriseId, cancellationToken, tracking: false)
            ?? StoreSettings.CreateDefault(enterpriseId);
    }

    /// <summary>
    /// Mesma leitura de GetCurrentAsync para uma loja informada, sem depender da requisição (worker e webhook de pagamento).
    /// </summary>
    public async Task<StoreSettings> GetForEnterpriseAsync(long enterpriseId, CancellationToken cancellationToken)
    {
        return await settingsRepository.FindAsync(settings => settings.EnterpriseId == enterpriseId, cancellationToken,
                ignoreQueryFilters: true, tracking: false)
            ?? StoreSettings.CreateDefault(enterpriseId);
    }

    public Task<bool> IsStoreActiveAsync(CancellationToken cancellationToken)
    {
        var enterpriseId = tenantProvider.GetEnterpriseId();

        return enterpriseRepository.ExistsAsync(enterprise => enterprise.Id == enterpriseId && enterprise.IsActive, cancellationToken);
    }
}
