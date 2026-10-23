using SharedCommerce.Application.Security;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class LogoutCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<LogoutCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(LogoutCommand command, CancellationToken ct)
    {
        var token = command.Token;
        var customer = command.Customer;

        var hash = OpaqueToken.Hash(token);
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        if (customer)
        {
            var tenant = actor.EnterpriseId;
            var record = await data.CustomerRefreshTokens.FindAsync(x => x.TokenHash == hash && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true, tracking: false);

            if (record is not null)
            {
                await LockCustomerAsync(record.CustomerId, ct);
                await RevokeFamilyAsync(record.CustomerId, record.FamilyId, true, ct);
            }
        }
        else
        {
            var record = await data.UserRefreshTokens.FindAsync(x => x.TokenHash == hash, ct, tracking: false);

            if (record is not null)
            {
                await LockUserAsync(record.UserId, ct);
                await RevokeFamilyAsync(record.UserId, record.FamilyId, false, ct);
            }
        }

        await transaction.CommitAsync(ct);

        return true;
    }
}
