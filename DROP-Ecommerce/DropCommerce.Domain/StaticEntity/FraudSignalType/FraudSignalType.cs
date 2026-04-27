namespace DropCommerce.Domain.StaticEntity;

public sealed class FraudSignalType : BaseStaticEntity
{
    public static readonly FraudSignalType DuplicateIP = new(1, "IP duplicado");
    public static readonly FraudSignalType DuplicateDevice = new(2, "Dispositivo duplicado");
    public static readonly FraudSignalType BotBehavior = new(3, "Comportamento de bot");
    public static readonly FraudSignalType MultipleAccounts = new(4, "Múltiplas contas");
    public static readonly FraudSignalType VPNDetected = new(5, "VPN detectada");
    public static readonly FraudSignalType AbnormalSpeed = new(6, "Velocidade anormal");

    private FraudSignalType(long id, string description) : base(id, description) { }
}