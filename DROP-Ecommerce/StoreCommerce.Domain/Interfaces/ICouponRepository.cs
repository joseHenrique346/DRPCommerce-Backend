using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface ICouponRepository : IRepository<Coupon>
{
    /// <summary>
    /// Busca pelo código normalizado (sem espaços, maiúsculo) entre os cupons não excluídos da loja.
    /// </summary>
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna os códigos já usados por cupons não excluídos da loja, desconsiderando os ids informados.
    /// </summary>
    Task<List<string>> GetExistingCodesAsync(IReadOnlyCollection<string> codes, IReadOnlyCollection<long> excludeIds, CancellationToken cancellationToken = default);

    Task<(List<Coupon> Items, int TotalCount)> SearchAsync(
        string? code, long? couponTypeId, bool? isActive, CouponAvailability? availability, DateTime now,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
