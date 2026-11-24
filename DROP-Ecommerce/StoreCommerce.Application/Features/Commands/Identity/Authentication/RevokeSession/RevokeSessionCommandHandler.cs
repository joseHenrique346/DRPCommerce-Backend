using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RevokeSessionCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RevokeSessionCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(RevokeSessionCommand command, CancellationToken ct)
    {
        var familyId = command.FamilyId;
        var customer = command.Customer;

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var account = await CurrentAccountAsync(customer, true, ct);
        await RevokeFamilyAsync(account.Id, familyId, customer, ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
