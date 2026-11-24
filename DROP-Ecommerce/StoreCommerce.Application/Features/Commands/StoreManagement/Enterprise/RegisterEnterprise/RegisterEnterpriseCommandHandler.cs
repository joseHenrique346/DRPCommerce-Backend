using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Contracts.Storefront;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RegisterEnterpriseCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RegisterEnterpriseCommand, SafeEnterprise>(context)
{
    protected override async Task<SafeEnterprise> ExecuteAsync(RegisterEnterpriseCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var userId = actor.UserId;
        var slug = StorefrontSlug.Normalize(request.Slug) ??
            throw new SecurityFailure(400, "Enterprise.InvalidSlug", "Identificador público da loja inválido ou reservado.");

        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await LockUserAsync(userId, ct);
        var user = await data.Users.GetRequiredAsync(x => x.Id == userId && x.IsActive, ct);

        if (await data.Enterprises.ExistsAsync(Enterprise => Enterprise.StorefrontSlug == slug, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(409, "Enterprise.SlugUnavailable", "Este endereço público já está em uso.");

        var enterprise = Enterprise.Create(request.TradeName, request.LegalName, EnterpriseEmail.Create(request.Email),
            EnterprisePhone.Create(request.Phone), request.AddressLine, request.City, request.StateId, request.ZipCode, request.Country, true);
        enterprise.AssignStorefrontSlug(slug);
        data.Enterprises.Add(enterprise);
        await unitOfWork.CommitAsync(ct);
        var membership = EnterpriseMembership.Create(enterprise.Id, user.Id, 1, true, Now);
        data.EnterpriseMemberships.Add(membership);
        data.StoreSettings.Add(StoreSettings.CreateDefault(enterprise.Id));
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return new(enterprise.Id, enterprise.TradeName, enterprise.IsActive, enterprise.PlanId, enterprise.ContractRevision,
            slug, StorefrontUrls.Page(Options.StorefrontBaseUrl, slug));
    }
}
