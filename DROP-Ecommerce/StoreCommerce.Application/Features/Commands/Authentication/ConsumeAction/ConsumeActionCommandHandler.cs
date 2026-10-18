using Commerce.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ConsumeActionCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<ConsumeActionCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(ConsumeActionCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var purpose = command.Purpose;
        var customer = command.Customer;

        if (!delivery.IsAvailable)
            throw new SecurityFailure(503, "Identity.ActionDeliveryDisabled", "Confirmação e recuperação de senha indisponíveis nesta etapa.");

        if (purpose == AccountActionPurpose.ResetPassword)
            AccountPasswordPolicy.Validate(request.NewPassword!);

        var hash = OpaqueToken.Hash(request.Token);
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        IAuthenticationAccount account;

        if (customer)
        {
            var tenant = actor.EnterpriseId;
            var reference = await data.CustomerActionTokens.FindAsync(
                x => x.TokenHash == hash && x.EnterpriseId == tenant,
                ct,
                ignoreQueryFilters: true,
                tracking: false) ?? throw InvalidToken();
            await LockCustomerAsync(reference.CustomerId, ct);
            var token = await data.CustomerActionTokens.GetRequiredAsync(x => x.Id == reference.Id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true);
            account = await data.Customers.GetRequiredAsync(x => x.Id == token.CustomerId && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true);

            if (!token.IsUsable(Now, account.SecurityVersion, purpose) || !account.IsActive || ((Customer)account).IsDeleted)
                throw InvalidToken();

            token.Consume(Now);
        }
        else
        {
            var reference = await data.UserActionTokens.FindAsync(x => x.TokenHash == hash, ct, tracking: false) ?? throw InvalidToken();
            await LockUserAsync(reference.UserId, ct);
            var token = await data.UserActionTokens.GetRequiredAsync(x => x.Id == reference.Id, ct);
            account = await data.Users.GetRequiredAsync(x => x.Id == token.UserId, ct);

            if (!token.IsUsable(Now, account.SecurityVersion, purpose) || !account.IsActive)
                throw InvalidToken();

            token.Consume(Now);
        }

        if (purpose == AccountActionPurpose.ConfirmEmail)
            account.ConfirmEmail();
        else
        {
            account.SetPassword(passwords.Hash(account, request.NewPassword!));
            await RevokeAllAsync(account.Id, customer, ct);
        }

        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
