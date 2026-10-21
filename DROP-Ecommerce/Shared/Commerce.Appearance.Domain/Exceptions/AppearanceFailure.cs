namespace Commerce.Appearance.Domain;

public sealed class AppearanceFailure(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
}
