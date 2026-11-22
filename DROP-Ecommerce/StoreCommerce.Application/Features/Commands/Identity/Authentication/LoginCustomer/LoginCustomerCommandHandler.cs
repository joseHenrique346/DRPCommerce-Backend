using SharedCommerce.Application.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class LoginCustomerCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<LoginCustomerCommand, SessionResult>(context)
{
    protected override async Task<SessionResult> ExecuteAsync(LoginCustomerCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var tenant = actor.EnterpriseId;
        var email = NormalizeEmail(request.Email);
        var id = await data.Customers.FindValueAsync(x => x.EnterpriseId == tenant && x.NormalizedEmail == email, x => (long?)x.Id, ct, ignoreQueryFilters: true);

        if (!id.HasValue)
        {
            passwords.VerifyDummy(request.Password ?? "", true);
            throw InvalidCredentials();
        }

        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await LockCustomerAsync(id.Value, ct);
        var customer = await data.Customers.GetRequiredAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true);
        await VerifyLoginAsync(customer, request.Password, ct);
        var token = OpaqueToken.Create();
        var expires = Now.AddDays(Options.RefreshFamilyDays);
        var family = Guid.NewGuid();
        data.CustomerRefreshTokens.Add(CustomerRefreshToken.Create(customer, OpaqueToken.Hash(token), family, expires));
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);
        var access = jwt.IssueCustomer(customer, family);

        return new(access.Token, access.Expires, false, [], token, expires);
    }
}
