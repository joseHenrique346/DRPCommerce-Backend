using Commerce.Security;
using MediatR;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public abstract class IdentityHandlerBase<TRequest, TResponse>(IdentityHandlerContext context) : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : IIdentityRequest<TResponse>
{
    protected IIdentityRepositories data => context.Repositories;
    protected IIdentityUnitOfWork unitOfWork => context.UnitOfWork;
    protected ICurrentIdentity actor => context.Actor;
    protected IIdentityOperationScope scope => context.Scope;
    protected IAccountPasswordHasher passwords => context.Passwords;
    protected ISessionTokenIssuer jwt => context.Tokens;
    protected SecurityOptions Options => context.Options;
    protected TimeProvider time => context.Time;
    protected IAccountActionDelivery delivery => context.Delivery;
    protected IDomainOwnershipVerifier domainVerifier => context.DomainVerifier;
    protected DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await ExecuteAsync(request, cancellationToken);

            return Result<TResponse>.Success(result);
        }
        catch (SecurityFailure failure)
        {
            return Result<TResponse>.Failure(IdentityError.FromFailure(failure));
        }
    }

    protected abstract Task<TResponse> ExecuteAsync(TRequest command, CancellationToken ct);

    protected static SecurityFailure NotFound()
    {
        return new(404, "Access.NotFound", "Registro não encontrado.");
    }

    protected static SecurityFailure InvalidLink()
    {
        return new(400, "Access.InvalidLink", "Link inválido, expirado, revogado ou já utilizado.");
    }

    protected Task LockEnterpriseAsync(long tenant, CancellationToken ct)
    {
        return data.Enterprises.LockAsync(tenant, ct);
    }

    protected async Task<IIdentityTransaction> BeginOwnerOperationAsync(CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var ownerUserId = actor.UserId;
            await data.Users.LockAsync(ownerUserId, ct);
            await LockEnterpriseAsync(tenant, ct);

            if (!await data.EnterpriseMemberships.ExistsAsync(x => x.Id == actor.MembershipId &&
                x.UserId == actor.UserId && x.EnterpriseId == tenant && x.IsActive && x.KindId == 1 && x.User.IsActive && x.Enterprise.IsActive, ct, ignoreQueryFilters: true))
                throw new SecurityFailure(403, "Access.PrimaryOwnerRequired", "Apenas o dono principal pode gerir os acessos.");

            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();


            throw;
        }
    }

    protected async Task ValidateRolesAsync(IReadOnlyCollection<long> ids, long tenant, CancellationToken ct)
    {
        if (ids.Count != ids.Distinct().Count() || ids.Any(x => x < 1) ||
            await data.AccessRoles.CountAsync(x => ids.Contains(x.Id) && x.EnterpriseId == tenant && x.IsActive, ct, ignoreQueryFilters: true) != ids.Count)
            throw new SecurityFailure(400, "Access.InvalidRoles", "Perfis inválidos, inativos ou de outra loja.");
    }

    protected async Task InvalidateRoleMembersAsync(long roleId, long tenant, CancellationToken ct)
    {
        var members = await data.EnterpriseMemberships.GetWithRoleAsync(roleId, tenant, ct);

        foreach (var member in members)
            member.InvalidateAccess();
    }

    protected async Task<IAuthenticationAccount> CurrentAccountAsync(bool customer, bool locked, CancellationToken ct)
    {
        if (customer)
        {
            var id = actor.CustomerId;
            var tenant = actor.EnterpriseId;

            if (locked)
                await LockCustomerAsync(id, ct);

            return await data.Customers.FindAsync(x => x.Id == id && x.EnterpriseId == tenant && x.IsActive && !x.IsDeleted, ct, ignoreQueryFilters: true)
                ?? throw InvalidCredentials();
        }

        var userId = actor.UserId;

        if (locked)
            await LockUserAsync(userId, ct);

        return await data.Users.FindAsync(x => x.Id == userId && x.IsActive, ct) ?? throw InvalidCredentials();
    }

    protected static SecurityFailure InvalidCredentials()
    {
        return new(401, "Identity.InvalidCredentials", "Credenciais inválidas ou acesso temporariamente indisponível.");
    }

    protected static SecurityFailure InvalidToken()
    {
        return new(401, "Identity.InvalidToken", "Token inválido, expirado ou revogado.");
    }

    protected static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 255)
            throw new SecurityFailure(400, "Identity.InvalidEmail", "E-mail inválido.");

        return UserEmail.Create(email).Value.Trim().ToUpperInvariant();
    }

    protected Task LockUserAsync(long id, CancellationToken ct)
    {
        return data.Users.LockAsync(id, ct);
    }

    protected Task LockCustomerAsync(long id, CancellationToken ct)
    {
        return data.Customers.LockAsync(id, ct);
    }

    protected async Task VerifyLoginAsync(IAuthenticationAccount account, string password, CancellationToken ct)
    {
        var now = Now;
        var available = account.IsActive && account is not Customer { IsDeleted: true } &&
            (!Options.RequireConfirmedEmail || account.EmailConfirmed) && !(account.LockoutUntil > now);
        var verified = passwords.Verify(account, password, out var rehash);

        if (!available || !verified)
        {
            if (available && !verified)
                account.RecordFailedLogin(now, Options.FailedLoginThreshold, Options.InitialLockoutSeconds, Options.MaximumLockoutSeconds, Options.FailureResetMinutes);

            await unitOfWork.CommitAsync(ct);


            // Preserve failed-login counters when rejecting the request.
            if (unitOfWork.CurrentTransaction is { } transaction)
                await transaction.CommitAsync(ct);

            throw InvalidCredentials();
        }

        if (rehash)
            account.SetPassword(passwords.Hash(account, password), false);

        account.RecordSuccessfulLogin(now);
    }

    protected Task<List<EnterpriseMembership>> ActiveMembershipsAsync(long userId, CancellationToken ct)
    {
        return data.EnterpriseMemberships.GetActiveForUserAsync(userId, ct);
    }

    protected async Task<SessionResult> UserSessionAsync(User user, Guid family, string token, DateTime expires, EnterpriseMembership? membership, List<EnterpriseMembership> memberships, CancellationToken ct)
    {
        var access = await jwt.IssueUserAsync(user, family, membership, ct);

        return new(access.Token, access.Expires, membership is null && memberships.Count > 0,
            memberships.Select(x => new MembershipChoice(x.Id, x.EnterpriseId, x.Enterprise.TradeName, x.KindId)).ToList(), token, expires);
    }

    protected Task RevokeFamilyAsync(long accountId, Guid family, bool customer, CancellationToken ct)
    {
        return data.Sessions.RevokeFamilyAsync(accountId, family, customer, Now, ct);
    }

    protected Task RevokeAllAsync(long accountId, bool customer, CancellationToken ct)
    {
        return data.Sessions.RevokeAllAsync(accountId, customer, Now, ct);
    }
}
