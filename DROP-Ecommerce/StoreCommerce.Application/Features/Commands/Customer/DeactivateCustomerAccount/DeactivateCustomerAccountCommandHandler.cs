using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeactivateCustomerAccountCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<DeactivateCustomerAccountCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(DeactivateCustomerAccountCommand command, CancellationToken ct)
    {
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var customer = (Customer)await CurrentAccountAsync(true, true, ct);
        customer.SoftDelete();
        customer.InvalidateSessions();
        await RevokeAllAsync(customer.Id, true, ct);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
