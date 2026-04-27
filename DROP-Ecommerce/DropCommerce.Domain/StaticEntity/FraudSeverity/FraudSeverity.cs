namespace DropCommerce.Domain.StaticEntity;

public sealed class FraudSeverity : BaseStaticEntity
{
    public static readonly FraudSeverity Low = new(1, "Baixa");
    public static readonly FraudSeverity Medium = new(2, "Média");
    public static readonly FraudSeverity High = new(3, "Alta");
    public static readonly FraudSeverity Critical = new(4, "Crítica");

    private FraudSeverity(long id, string description) : base(id, description) { }
}