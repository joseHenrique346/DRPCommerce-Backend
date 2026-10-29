using System.Text.RegularExpressions;
using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Domain.Entity;

public class Coupon : BaseEntity, ITenantEntity, ISoftDeletable
{
    #region Constants
    public const int CodeMinLength = 3;
    public const int CodeMaxLength = 50;
    public const string CodePattern = "^[A-Z0-9_-]+$";
    public const decimal MaxPercentage = 100m;
    public const int MaxConditions = 4;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public long CouponTypeId { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal MaxDiscountCap { get; private set; }
    public int? MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSingleUse { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public CouponType CouponType { get; private set; }
    private readonly List<CouponCondition> _listCondition = [];
    public IReadOnlyCollection<CouponCondition> ListCondition => _listCondition.AsReadOnly();
    private readonly List<Order> _listOrder = [];
    public IReadOnlyCollection<Order> ListOrder => _listOrder.AsReadOnly();
    #endregion

    #endregion

    #region Constructor
    protected Coupon() { }

    private Coupon(long enterpriseId, bool isActive)
    {
        EnterpriseId = enterpriseId;
        UsedCount = 0;
        IsActive = isActive;
    }
    #endregion

    #region Functions
    /// <summary>
    /// Normaliza o código digitado pelo cliente (sem espaços e em maiúsculas), garantindo comparação sem diferenciar caixa.
    /// </summary>
    public static string NormalizeCode(string? code) => Regex.Replace(code ?? string.Empty, @"\s+", string.Empty).ToUpperInvariant();

    public static Coupon Create(long enterpriseId, string code, long couponTypeId, decimal discountValue, decimal maxDiscountCap, int? maxUses, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt, IReadOnlyCollection<CouponCondition> conditions)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));

        var coupon = new Coupon(enterpriseId, isActive);
        coupon.ApplyTerms(code, couponTypeId, discountValue, maxDiscountCap, maxUses, isSingleUse, startsAt, expiresAt, conditions);

        return coupon;
    }

    /// <summary>
    /// Atualiza o cupom. Depois de usado, somente a validade (ExpiresAt) e o limite de usos (MaxUses) podem mudar,
    /// para que pedidos já concluídos continuem coerentes com as regras que os aprovaram.
    /// </summary>
    public void UpdateDetails(string code, long couponTypeId, decimal discountValue, decimal maxDiscountCap, int? maxUses, bool isSingleUse, DateTime startsAt, DateTime expiresAt, IReadOnlyCollection<CouponCondition> conditions)
    {
        if (WouldChangeLockedTerms(code, couponTypeId, discountValue, maxDiscountCap, isSingleUse, startsAt, conditions))
            throw new ArgumentException("Cupom já utilizado: apenas a validade e o limite de usos podem ser alterados.");

        if (maxUses.HasValue && maxUses.Value < UsedCount)
            throw new ArgumentException($"{nameof(MaxUses)} cannot be lower than {nameof(UsedCount)}.");

        ApplyTerms(code, couponTypeId, discountValue, maxDiscountCap, maxUses, isSingleUse, startsAt, expiresAt, conditions);
    }

    public bool HasBeenUsed => UsedCount > 0;

    /// <summary>
    /// Indica se a alteração mexe em regras travadas após o primeiro uso (código, tipo, valor, teto, uso único, início e condições).
    /// </summary>
    public bool WouldChangeLockedTerms(string code, long couponTypeId, decimal discountValue, decimal maxDiscountCap, bool isSingleUse, DateTime startsAt, IReadOnlyCollection<CouponCondition> conditions)
    {
        if (!HasBeenUsed)
            return false;

        var cap = couponTypeId == CouponType.Percentage.Id ? maxDiscountCap : 0;

        return NormalizeCode(code) != Code || couponTypeId != CouponTypeId || discountValue != DiscountValue ||
            cap != MaxDiscountCap || isSingleUse != IsSingleUse || ToStoragePrecision(startsAt) != StartsAt ||
            conditions.Count != _listCondition.Count ||
            conditions.Any(condition => !_listCondition.Any(current => current.IsEquivalentTo(condition)));
    }

    public void SetActive(bool active) => IsActive = active;

    public bool IsPercentage => CouponTypeId == CouponType.Percentage.Id;

    public CouponAvailability GetAvailability(DateTime now)
    {
        if (!IsActive || IsDeleted) return CouponAvailability.Inactive;
        if (ExpiresAt <= now) return CouponAvailability.Expired;
        if (MaxUses.HasValue && UsedCount >= MaxUses.Value) return CouponAvailability.Exhausted;
        if (StartsAt > now) return CouponAvailability.Scheduled;

        return CouponAvailability.Active;
    }

    /// <summary>
    /// Verifica se o cupom pode ser usado agora: ativo, vigente, com usos disponíveis e, se for de uso único,
    /// ainda não resgatado pelo cliente.
    /// </summary>
    public CouponEvaluation? CheckAvailability(DateTime now, CouponCustomerHistory? history)
    {
        var failure = GetAvailability(now) switch
        {
            CouponAvailability.Inactive => CouponEvaluation.Invalid("Coupon.Inactive", "Cupom inativo."),
            CouponAvailability.Expired => CouponEvaluation.Invalid("Coupon.Expired", "Cupom expirado."),
            CouponAvailability.Exhausted => CouponEvaluation.Invalid("Coupon.Exhausted", "Cupom esgotado."),
            CouponAvailability.Scheduled => CouponEvaluation.Invalid("Coupon.NotStarted", "Cupom ainda não está vigente."),
            _ => null
        };

        if (failure is not null || !IsSingleUse)
            return failure;

        if (history is null)
            return CouponEvaluation.Invalid("Coupon.CustomerRequired", "Identifique o cliente para usar este cupom.");

        return history.HasRedeemedCoupon
            ? CouponEvaluation.Invalid("Coupon.AlreadyUsed", "Este cupom já foi utilizado por este cliente.")
            : null;
    }

    /// <summary>
    /// Verifica as condições ligadas ao histórico do cliente (clientes novos, recorrentes, total já gasto).
    /// </summary>
    public CouponEvaluation? CheckCustomerConditions(CouponCustomerHistory? history)
    {
        foreach (var condition in _listCondition.Where(condition => condition.IsCustomerScoped))
        {
            if (history is null)
                return CouponEvaluation.Invalid("Coupon.CustomerRequired", "Identifique o cliente para usar este cupom.");

            var actual = condition.CouponConditionTypeId == CouponConditionType.CustomerOrderCount.Id
                ? history.CompletedOrders
                : history.TotalSpent;

            if (!condition.IsSatisfiedBy(actual))
                return ConditionNotMet(condition);
        }

        return null;
    }

    /// <summary>
    /// Verifica as condições ligadas ao pedido (valor e quantidade de itens).
    /// </summary>
    public CouponEvaluation? CheckOrderConditions(decimal subTotal, int itemQuantity)
    {
        foreach (var condition in _listCondition.Where(condition => !condition.IsCustomerScoped))
        {
            var actual = condition.CouponConditionTypeId == CouponConditionType.OrderSubTotal.Id ? subTotal : itemQuantity;

            if (!condition.IsSatisfiedBy(actual))
                return ConditionNotMet(condition);
        }

        return null;
    }

    /// <summary>
    /// Calcula o desconto sobre o subtotal. Retorna zero enquanto as condições do pedido não forem atendidas.
    /// Percentual: limitado ao teto (MaxDiscountCap igual a zero significa sem teto). Valor fixo: nunca excede o subtotal.
    /// </summary>
    public decimal CalculateDiscount(decimal subTotal, int itemQuantity)
    {
        if (subTotal <= 0 || CheckOrderConditions(subTotal, itemQuantity) is not null)
            return 0;

        var discount = IsPercentage
            ? Math.Round(subTotal * DiscountValue / 100m, 2, MidpointRounding.AwayFromZero)
            : DiscountValue;

        if (IsPercentage && MaxDiscountCap > 0)
            discount = Math.Min(discount, MaxDiscountCap);

        return Math.Min(discount, subTotal);
    }

    /// <summary>
    /// Avaliação completa: disponibilidade, condições do cliente, condições do pedido e desconto resultante.
    /// </summary>
    public CouponEvaluation Evaluate(DateTime now, decimal subTotal, int itemQuantity, CouponCustomerHistory? history)
    {
        var failure = CheckAvailability(now, history) ?? CheckCustomerConditions(history) ?? CheckOrderConditions(subTotal, itemQuantity);

        if (failure is not null)
            return failure;

        var discount = CalculateDiscount(subTotal, itemQuantity);

        return discount > 0
            ? CouponEvaluation.Valid(discount)
            : CouponEvaluation.Invalid("Coupon.NoDiscount", "O pedido não possui valor para aplicar o desconto.");
    }

    /// <summary>
    /// Registra um uso. A coluna UsedCount é token de concorrência: dois resgates simultâneos não ultrapassam MaxUses.
    /// </summary>
    public void RegisterUse(DateTime now)
    {
        if (GetAvailability(now) != CouponAvailability.Active)
            throw new ArgumentException("Cupom inativo, fora da vigência ou sem usos disponíveis.");

        UsedCount++;
    }

    public void ReleaseUse()
    {
        if (UsedCount > 0)
            UsedCount--;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    private void ApplyTerms(string code, long couponTypeId, decimal discountValue, decimal maxDiscountCap, int? maxUses, bool isSingleUse, DateTime startsAt, DateTime expiresAt, IReadOnlyCollection<CouponCondition> conditions)
    {
        var normalizedCode = NormalizeCode(code);

        BaseValidate.ValidateNotNullOrEmpty(normalizedCode, nameof(Code));
        BaseValidate.ValidateLength(normalizedCode, CodeMinLength, CodeMaxLength, nameof(Code));

        if (!Regex.IsMatch(normalizedCode, CodePattern))
            throw new ArgumentException($"{nameof(Code)} aceita apenas letras sem acento, números, hífen e sublinhado.");

        if (!BaseStaticEntity.GetAll<CouponType>().Any(type => type.Id == couponTypeId))
            throw new ArgumentException($"{nameof(CouponTypeId)} is invalid.");

        BaseValidate.ValidatePositive(discountValue, nameof(DiscountValue));

        if (couponTypeId == CouponType.Percentage.Id && discountValue > MaxPercentage)
            throw new ArgumentException($"{nameof(DiscountValue)} cannot be greater than {MaxPercentage}% for percentage coupons.");

        BaseValidate.ValidatePositiveOrZero(maxDiscountCap, nameof(MaxDiscountCap));
        BaseValidate.ValidateNullablePositive(maxUses, nameof(MaxUses));
        BaseValidate.ValidateGreaterThan(expiresAt, startsAt, nameof(ExpiresAt));
        BaseValidate.ValidateNotNull(conditions, nameof(ListCondition));

        if (conditions.Count > MaxConditions || conditions.GroupBy(condition => condition.CouponConditionTypeId).Any(group => group.Count() > 1))
            throw new ArgumentException($"Use no máximo uma condição por tipo ({MaxConditions} no total).");

        Code = normalizedCode;
        CouponTypeId = couponTypeId;
        DiscountValue = discountValue;
        // O teto só faz sentido em cupons percentuais; no valor fixo o desconto já é o próprio valor.
        MaxDiscountCap = couponTypeId == CouponType.Percentage.Id ? maxDiscountCap : 0;
        MaxUses = maxUses;
        IsSingleUse = isSingleUse;
        StartsAt = ToStoragePrecision(startsAt);
        ExpiresAt = ToStoragePrecision(expiresAt);

        if (_listCondition.Count != conditions.Count || conditions.Any(condition => !_listCondition.Any(current => current.IsEquivalentTo(condition))))
        {
            _listCondition.Clear();
            _listCondition.AddRange(conditions);
        }
    }

    // O PostgreSQL guarda microssegundos; truncar evita que a mesma data pareça diferente depois de salva.
    private static DateTime ToStoragePrecision(DateTime value) =>
        new(value.Ticks - value.Ticks % 10, value.Kind);

    private static CouponEvaluation ConditionNotMet(CouponCondition condition) =>
        CouponEvaluation.Invalid("Coupon.ConditionNotMet", $"Cupom válido apenas quando {condition.Describe()}.");
    #endregion
}
