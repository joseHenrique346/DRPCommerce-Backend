using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateUserProfileCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<UpdateUserProfileCommand, UserResponse>(context)
{
    protected override async Task<UserResponse> ExecuteAsync(UpdateUserProfileCommand command, CancellationToken ct)
    {
        var request = command.Request;

        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var user = (User)await CurrentAccountAsync(false, true, ct);
        user.UpdateDetails(request.FullName, user.Email, user.IsActive);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return UserResponse.FromEntity(user);
    }
}
