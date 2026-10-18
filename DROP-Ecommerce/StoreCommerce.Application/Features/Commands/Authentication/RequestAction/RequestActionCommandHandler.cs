using Commerce.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RequestActionCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RequestActionCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(RequestActionCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var purpose = command.Purpose;
        var customer = command.Customer;

        // Delivery is intentionally disabled in this stage. Never expose a reset token in a public response.
        if (!delivery.IsAvailable)
            throw new SecurityFailure(503, "Identity.ActionDeliveryDisabled", "Confirmação e recuperação de senha indisponíveis nesta etapa.");

        var email = NormalizeEmail(request.Email);
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var raw = OpaqueToken.Create();
        var expires = Now.AddMinutes(Options.ActionTokenMinutes);
        string publicBaseUrl;
        string recipient;

        if (customer)
        {
            var tenant = actor.EnterpriseId;
            var reference = await data.Customers.FindAsync(x => x.EnterpriseId == tenant && x.NormalizedEmail == email, ct, ignoreQueryFilters: true, tracking: false);

            if (reference is null || !reference.IsActive || reference.IsDeleted)
                return true;

            await LockCustomerAsync(reference.Id, ct);
            var account = await data.Customers.GetRequiredAsync(x => x.Id == reference.Id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true);

            foreach (var token in await data.CustomerActionTokens.ListAsync(
                x => x.CustomerId == account.Id && x.Purpose == purpose && x.ConsumedAt == null && x.RevokedAt == null,
                ct,
                ignoreQueryFilters: true))
                token.Revoke(Now);

            data.CustomerActionTokens.Add(CustomerActionToken.Create(account, OpaqueToken.Hash(raw), purpose, expires));
            recipient = account.Email.Value;
            publicBaseUrl = "https://" + HostName.Normalize(actor.Host);
        }
        else
        {
            var reference = await data.Users.FindAsync(x => x.NormalizedEmail == email, ct, tracking: false);

            if (reference is null || !reference.IsActive)
                return true;

            await LockUserAsync(reference.Id, ct);
            var account = await data.Users.GetRequiredAsync(x => x.Id == reference.Id, ct);

            foreach (var token in await data.UserActionTokens.ListAsync(
                x => x.UserId == account.Id && x.Purpose == purpose && x.ConsumedAt == null && x.RevokedAt == null,
                ct))
                token.Revoke(Now);

            data.UserActionTokens.Add(UserActionToken.Create(account, OpaqueToken.Hash(raw), purpose, expires));
            recipient = account.Email.Value;
            publicBaseUrl = Options.BackofficeUrl;
        }

        await unitOfWork.CommitAsync(ct);
        await delivery.DeliverAsync(recipient, raw, purpose, publicBaseUrl, ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
