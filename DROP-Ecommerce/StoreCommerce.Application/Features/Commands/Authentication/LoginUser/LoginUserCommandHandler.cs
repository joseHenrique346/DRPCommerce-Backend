using SharedCommerce.Application.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class LoginUserCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<LoginUserCommand, SessionResult>(context)
{
    protected override async Task<SessionResult> ExecuteAsync(LoginUserCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var email = NormalizeEmail(request.Email);
        var id = await data.Users.FindValueAsync(x => x.NormalizedEmail == email, x => (long?)x.Id, ct);

        if (!id.HasValue)
        {
            passwords.VerifyDummy(request.Password ?? "", false);
            throw InvalidCredentials();
        }

        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await LockUserAsync(id.Value, ct);
        var user = await data.Users.GetRequiredAsync(x => x.Id == id, ct);
        await VerifyLoginAsync(user, request.Password, ct);
        var memberships = await ActiveMembershipsAsync(user.Id, ct);
        var membership = memberships.Count == 1 ? memberships[0] : null;
        var token = OpaqueToken.Create();
        var expires = Now.AddDays(Options.RefreshFamilyDays);
        var family = Guid.NewGuid();
        data.UserRefreshTokens.Add(UserRefreshToken.Create(user, OpaqueToken.Hash(token), family, expires, membership?.Id));
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return await UserSessionAsync(user, family, token, expires, membership, memberships, ct);
    }
}
