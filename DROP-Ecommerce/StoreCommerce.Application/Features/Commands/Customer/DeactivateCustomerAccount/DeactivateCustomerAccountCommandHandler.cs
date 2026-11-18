using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeactivateCustomerAccountCommandHandler(IdentityHandlerContext context, IAccountDeletionStore deletion,
    IProductRepository products) : IdentityHandlerBase<DeactivateCustomerAccountCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(DeactivateCustomerAccountCommand command, CancellationToken ct)
    {
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(actor.EnterpriseId, ct);
        var customer = (Customer)await CurrentAccountAsync(true, true, ct);
        await VerifyLoginAsync(customer, command.Request.CurrentPassword, ct);
        var eligibility = await deletion.InspectCustomerAsync(customer.EnterpriseId, customer.Id, Now, ct);

        if (!eligibility.CanDelete)
            throw new SecurityFailure(409, eligibility.Blockers[0].Code, eligibility.Blockers[0].Message);

        await deletion.EraseCustomerRelatedDataAsync(customer.EnterpriseId, customer.Id, ct);
        customer.ErasePersonalData(Now);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
