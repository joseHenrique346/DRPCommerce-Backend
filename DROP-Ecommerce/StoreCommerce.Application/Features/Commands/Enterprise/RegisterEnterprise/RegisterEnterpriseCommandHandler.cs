using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using System.Text.RegularExpressions;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RegisterEnterpriseCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RegisterEnterpriseCommand, SafeEnterprise>(context)
{
    protected override async Task<SafeEnterprise> ExecuteAsync(RegisterEnterpriseCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var userId = actor.UserId;
        var baseDomain = HostName.Normalize(Options.StorefrontBaseDomain);
        var slug = request.Slug?.Trim().ToLowerInvariant() ?? "";

        if (!Regex.IsMatch(slug, "^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.CultureInvariant) ||
            slug is "www" or "api" or "admin" or "backoffice" or "mail" or "internal")
            throw new SecurityFailure(400, "Enterprise.InvalidSlug", "Subdomínio inválido ou reservado.");

        var host = HostName.Normalize(slug + "." + baseDomain);
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await LockUserAsync(userId, ct);
        var user = await data.Users.GetRequiredAsync(x => x.Id == userId && x.IsActive, ct);

        if (await data.EnterpriseDomains.ExistsAsync(x => x.NormalizedHost == host, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(409, "Enterprise.DomainUnavailable", "Este subdomínio já está em uso.");

        var enterprise = Enterprise.Create(request.TradeName, request.LegalName, EnterpriseEmail.Create(request.Email),
            EnterprisePhone.Create(request.Phone), request.AddressLine, request.City, request.StateId, request.ZipCode, request.Country, true);
        data.Enterprises.Add(enterprise);
        await unitOfWork.CommitAsync(ct);
        var membership = EnterpriseMembership.Create(enterprise.Id, user.Id, 1, true, Now);
        var domain = EnterpriseDomain.Create(enterprise.Id, host, true, false);
        domain.Verify();
        domain.SetPublication(true, true);
        data.EnterpriseMemberships.Add(membership);
        data.EnterpriseDomains.Add(domain);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return new(enterprise.Id, enterprise.TradeName, enterprise.IsActive);
    }
}
