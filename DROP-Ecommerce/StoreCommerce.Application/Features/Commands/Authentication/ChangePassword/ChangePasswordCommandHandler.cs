using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ChangePasswordCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<ChangePasswordCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(ChangePasswordCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var customer = command.Customer;

        AccountPasswordPolicy.Validate(request.NewPassword);
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var account = await CurrentAccountAsync(customer, true, ct);

        if (!passwords.Verify(account, request.CurrentPassword, out _))
            throw InvalidCredentials();

        account.SetPassword(passwords.Hash(account, request.NewPassword));
        await RevokeAllAsync(account.Id, customer, ct);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
