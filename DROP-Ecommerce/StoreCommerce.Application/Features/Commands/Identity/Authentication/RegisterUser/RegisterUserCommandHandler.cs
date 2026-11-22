using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RegisterUserCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RegisterUserCommand, UserResponse>(context)
{
    protected override async Task<UserResponse> ExecuteAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var request = command.Request;

        AccountPasswordPolicy.Validate(request.Password);
        var email = NormalizeEmail(request.Email);

        if (await data.Users.ExistsAsync(x => x.NormalizedEmail == email, ct))
            throw new SecurityFailure(409, "Identity.AccountUnavailable", "Não é possível cadastrar esta conta.");

        var user = User.Create(request.FullName, UserEmail.Create(request.Email), true);
        user.SetPassword(passwords.Hash(user, request.Password), false);
        using var operation = scope.Begin();
        data.Users.Add(user);
        await unitOfWork.CommitAsync(ct);

        return UserResponse.FromEntity(user);
    }
}
