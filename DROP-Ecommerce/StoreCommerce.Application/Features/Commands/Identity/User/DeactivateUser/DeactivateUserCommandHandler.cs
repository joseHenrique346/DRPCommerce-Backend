using StoreCommerce.Application.Interfaces.Identity;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeactivateUserCommandHandler(IdentityHandlerContext context, IAccountDeletionStore deletion) : IdentityHandlerBase<DeactivateUserCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(DeactivateUserCommand command, CancellationToken ct)
    {
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var user = (User)await CurrentAccountAsync(false, true, ct);
        await VerifyLoginAsync(user, command.Request.CurrentPassword, ct);
        var eligibility = await deletion.InspectUserAsync(user.Id, ct);

        if (!eligibility.CanDelete)
            throw new SecurityFailure(409, eligibility.Blockers[0].Code, eligibility.Blockers[0].Message);

        await deletion.EraseUserRelatedDataAsync(user.Id, user.NormalizedEmail, Now, ct);
        user.ErasePersonalData(Now);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
