using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetUserQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<GetUserQuery, UserResponse>(context)
{
    protected override async Task<UserResponse> ExecuteAsync(GetUserQuery command, CancellationToken ct)
    {
        return UserResponse.FromEntity((User)await CurrentAccountAsync(false, false, ct));
    }
}
