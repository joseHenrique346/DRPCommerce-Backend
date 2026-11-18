using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetAccountDeletionEligibilityQuery(bool Customer) : IIdentityRequest<AccountDeletionEligibility>;

public sealed class GetAccountDeletionEligibilityQueryHandler(IdentityHandlerContext context, IAccountDeletionStore deletion)
    : IdentityHandlerBase<GetAccountDeletionEligibilityQuery, AccountDeletionEligibility>(context)
{
    protected override async Task<AccountDeletionEligibility> ExecuteAsync(GetAccountDeletionEligibilityQuery command, CancellationToken ct)
    {
        var account = await CurrentAccountAsync(command.Customer, false, ct);

        return command.Customer
            ? await deletion.InspectCustomerAsync(actor.EnterpriseId, account.Id, Now, ct)
            : await deletion.InspectUserAsync(account.Id, ct);
    }
}
