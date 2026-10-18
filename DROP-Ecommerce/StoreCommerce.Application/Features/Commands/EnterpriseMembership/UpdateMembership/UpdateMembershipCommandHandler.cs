using Commerce.Security;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateMembershipCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<UpdateMembershipCommand, EnterpriseMembershipResponse>(context)
{
    protected override async Task<EnterpriseMembershipResponse> ExecuteAsync(UpdateMembershipCommand command, CancellationToken ct)
    {
        var id = command.Id;
        var request = command.Request;

        if (request.KindId is not (2 or 3))
            throw new SecurityFailure(400, "Access.UseOwnershipTransfer", "Para alterar o dono principal, utilize a transferência de propriedade.");

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var member = await data.EnterpriseMemberships.FindAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();

        if (member.KindId == 1)
            throw new SecurityFailure(409, "Access.PrimaryOwnerProtected", "O dono principal não pode ser removido, rebaixado ou desativado.");

        if (request.IsActive && !await data.Users.ExistsAsync(x => x.Id == member.UserId && x.IsActive, ct))
            throw new SecurityFailure(409, "Access.UserInactive", "A conta global está inativa.");

        member.UpdateDetails(request.KindId, request.IsActive, member.JoinedAt);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return EnterpriseMembershipResponse.FromEntity(member);
    }
}
