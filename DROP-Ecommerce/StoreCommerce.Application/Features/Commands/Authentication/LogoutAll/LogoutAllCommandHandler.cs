using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class LogoutAllCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<LogoutAllCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(LogoutAllCommand command, CancellationToken ct)
    {
        var customer = command.Customer;

        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var account = await CurrentAccountAsync(customer, true, ct);
        account.InvalidateSessions();
        await RevokeAllAsync(account.Id, customer, ct);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
