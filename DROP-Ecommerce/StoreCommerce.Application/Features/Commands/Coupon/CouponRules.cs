using StoreCommerce.Application.Contracts.Coupon;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

internal static class CouponRules
{
    public static IReadOnlyCollection<CouponCondition> BuildConditions(IReadOnlyCollection<CouponConditionRequest>? conditions) =>
        (conditions ?? []).Select(condition => CouponCondition.Create(condition.TypeId, condition.OperatorId, condition.Value, condition.SecondValue)).ToList();

    /// <summary>
    /// Garante código único por loja: dentro do lote e contra os cupons não excluídos já cadastrados.
    /// </summary>
    public static async Task<Error?> ValidateUniqueCodesAsync(ICouponRepository repository, IReadOnlyCollection<(long Id, string Code)> coupons, CancellationToken cancellationToken)
    {
        var codes = coupons.Select(coupon => Coupon.NormalizeCode(coupon.Code)).ToList();
        var duplicated = codes.GroupBy(code => code).FirstOrDefault(group => group.Count() > 1)?.Key;

        if (duplicated is not null)
            return Error.Conflict("Coupon.DuplicateCode", $"O código {duplicated} aparece mais de uma vez na requisição.");

        var existing = await repository.GetExistingCodesAsync(codes, coupons.Select(coupon => coupon.Id).Where(id => id > 0).ToList(), cancellationToken);

        return existing.Count > 0
            ? Error.Conflict("Coupon.DuplicateCode", $"Já existe um cupom com o código {existing[0]} na loja.")
            : null;
    }
}
