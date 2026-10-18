using Commerce.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateCurrentEnterpriseCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<UpdateCurrentEnterpriseCommand, SafeEnterprise>(context)
{
    protected override async Task<SafeEnterprise> ExecuteAsync(UpdateCurrentEnterpriseCommand command, CancellationToken ct)
    {
        var request = command.Request;

        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await data.Enterprises.LockAsync(tenant, ct);

        if (!await data.EnterpriseMemberships.ExistsAsync(x => x.Id == actor.MembershipId && x.UserId == actor.UserId &&
            x.EnterpriseId == tenant && x.IsActive && x.KindId == 1 && x.User.IsActive, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(403, "Enterprise.PrimaryOwnerRequired", "Apenas o dono principal pode alterar a loja.");

        var enterprise = await data.Enterprises.GetRequiredAsync(x => x.Id == tenant, ct, ignoreQueryFilters: true);
        enterprise.UpdateInfo(request.TradeName, request.LegalName, EnterpriseEmail.Create(request.Email), EnterprisePhone.Create(request.Phone));
        enterprise.UpdateAddress(request.AddressLine, request.City, request.StateId, request.ZipCode, request.Country);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return new(enterprise.Id, enterprise.TradeName, enterprise.IsActive);
    }
}
