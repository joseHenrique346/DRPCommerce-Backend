namespace StoreCommerce.Domain.StaticEntity;

public sealed class State : BaseStaticEntity
{
    public static readonly State AC = new(1, "Acre");
    public static readonly State AL = new(2, "Alagoas");
    public static readonly State AP = new(3, "Amapá");
    public static readonly State AM = new(4, "Amazonas");
    public static readonly State BA = new(5, "Bahia");
    public static readonly State CE = new(6, "Ceará");
    public static readonly State DF = new(7, "Distrito Federal");
    public static readonly State ES = new(8, "Espírito Santo");
    public static readonly State GO = new(9, "Goiás");
    public static readonly State MA = new(10, "Maranhão");
    public static readonly State MT = new(11, "Mato Grosso");
    public static readonly State MS = new(12, "Mato Grosso do Sul");
    public static readonly State MG = new(13, "Minas Gerais");
    public static readonly State PA = new(14, "Pará");
    public static readonly State PB = new(15, "Paraíba");
    public static readonly State PR = new(16, "Paraná");
    public static readonly State PE = new(17, "Pernambuco");
    public static readonly State PI = new(18, "Piauí");
    public static readonly State RJ = new(19, "Rio de Janeiro");
    public static readonly State RN = new(20, "Rio Grande do Norte");
    public static readonly State RS = new(21, "Rio Grande do Sul");
    public static readonly State RO = new(22, "Rondônia");
    public static readonly State RR = new(23, "Roraima");
    public static readonly State SC = new(24, "Santa Catarina");
    public static readonly State SP = new(25, "São Paulo");
    public static readonly State SE = new(26, "Sergipe");
    public static readonly State TO = new(27, "Tocantins");

    private State(long id, string description) : base(id, description) { }
}
