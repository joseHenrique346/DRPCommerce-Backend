using SharedCommerce.Application.Security;
using Microsoft.AspNetCore.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Security.Hashers;

/// <summary>
/// Protege senhas com os hashers do Identity para cadastrar e autenticar contas sem guardar senhas em texto.
/// </summary>
public sealed class AccountPasswordHasher(IPasswordHasher<User> users, IPasswordHasher<Customer> customers) : IAccountPasswordHasher
{
    private readonly User dummy = User.Create("Unknown", UserEmail.Create("unknown@example.invalid"), false);
    private string? dummyHash;

    /// <summary>
    /// Calcula o hash da senha para armazenar a credencial sem expor seu valor original.
    /// </summary>
    public string Hash(IAuthenticationAccount account, string password)
    {
        AccountPasswordPolicy.Validate(password);

        return account switch
        {
            User u => users.HashPassword(u, password),
            Customer c => customers.HashPassword(c, password),
            _ => throw new ArgumentException("Conta inválida.")
        };
    }

    /// <summary>
    /// Confere a senha e sinaliza quando atualizar o hash, mantendo a verificação segura e compatível.
    /// </summary>
    public bool Verify(IAuthenticationAccount account, string password, out bool needsRehash)
    {
        needsRehash = false;

        if (string.IsNullOrEmpty(password) || password.Length > 128 || account.PasswordHash is null)
        {
            VerifyDummy(password ?? "", account is Customer);

            return false;
        }

        PasswordVerificationResult result;

        try
        {
            result = account switch
            {
                User u => users.VerifyHashedPassword(u, u.PasswordHash!, password),
                Customer c => customers.VerifyHashedPassword(c, c.PasswordHash!, password),
                _ => PasswordVerificationResult.Failed
            };
        }
        catch (FormatException)
        {
            result = PasswordVerificationResult.Failed;
        }

        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;

        return result != PasswordVerificationResult.Failed;
    }

    /// <summary>
    /// Executa uma verificação fictícia para reduzir diferenças de tempo que poderiam revelar a existência de uma conta.
    /// </summary>
    public void VerifyDummy(string password, bool customer)
    {
        var hash = LazyInitializer.EnsureInitialized(ref dummyHash, () => users.HashPassword(dummy, OpaqueToken.Create()));
        users.VerifyHashedPassword(dummy, hash, password.Length <= 128 ? password : "");
    }

}
