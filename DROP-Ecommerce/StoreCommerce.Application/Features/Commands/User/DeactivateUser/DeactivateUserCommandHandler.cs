using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeactivateUserCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<DeactivateUserCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(DeactivateUserCommand command, CancellationToken ct)
    {
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var user = (User)await CurrentAccountAsync(false, true, ct);

        if (await data.EnterpriseMemberships.ExistsAsync(x => x.UserId == user.Id && x.KindId == 1 && x.IsActive, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(409, "Identity.PrimaryOwnerInUse", "Transfira a propriedade de suas lojas, inclusive as pausadas, antes de desativar sua conta.");

        user.UpdateDetails(user.FullName, user.Email, false);
        user.InvalidateSessions();
        await RevokeAllAsync(user.Id, false, ct);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
