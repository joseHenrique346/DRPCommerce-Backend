namespace StoreCommerce.Application.Identity;

public sealed record DeleteAccountRequest(string CurrentPassword)
{
    public override string ToString()
    {
        return "[Solicitação de exclusão de conta]";
    }
}
