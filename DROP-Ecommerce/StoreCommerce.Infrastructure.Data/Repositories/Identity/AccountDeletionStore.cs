using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class AccountDeletionStore(StoreCommerceDbContext context, IIdentityOperationScope scope) : IAccountDeletionStore
{
    public async Task<AccountDeletionEligibility> InspectUserAsync(long userId, CancellationToken ct)
    {
        // Inclui vínculos inativos: a desativação de uma loja não transfere sua responsabilidade.
        var ownedStores = await context.EnterpriseMemberships.IgnoreQueryFilters()
            .CountAsync(Membership => Membership.UserId == userId && Membership.KindId == EnterpriseMembershipKind.PrimaryOwner.Id, ct);
        var blockers = new List<AccountDeletionBlocker>();

        if (ownedStores > 0)
            blockers.Add(new AccountDeletionBlocker("Identity.PrimaryOwnerInUse",
                "Transfira a propriedade de suas lojas, inclusive as pausadas, antes de excluir sua conta.", ownedStores));

        return new AccountDeletionEligibility(blockers.Count == 0, blockers);
    }

    public async Task<AccountDeletionEligibility> InspectCustomerAsync(long enterpriseId, long customerId, DateTime now, CancellationToken ct)
    {
        var blockers = new List<AccountDeletionBlocker>();
        var pendingPayments = await context.Transactions.IgnoreQueryFilters()
            .CountAsync(Transaction => Transaction.Order.EnterpriseId == enterpriseId && Transaction.CustomerId == customerId &&
                (Transaction.TransactionStatusId == TransactionStatus.Pending.Id || Transaction.TransactionStatusId == TransactionStatus.Authorized.Id), ct);
        var pendingOrders = await context.Orders.IgnoreQueryFilters()
            .CountAsync(Order => Order.EnterpriseId == enterpriseId && Order.CustomerId == customerId &&
                Order.OrderStatusId == OrderStatus.Pending.Id, ct);

        if (pendingPayments > 0 || pendingOrders > 0)
            blockers.Add(new AccountDeletionBlocker("AccountDeletion.PendingPayments",
                "Conclua ou cancele os pedidos e pagamentos pendentes antes de excluir a conta.", Math.Max(pendingPayments, pendingOrders)));

        var uncertainPayments = await context.PaymentAttempts.IgnoreQueryFilters()
            .CountAsync(Attempt => Attempt.EnterpriseId == enterpriseId && Attempt.CustomerId == customerId &&
                (Attempt.Status == PaymentCreationStatus.Processing || Attempt.Status == PaymentCreationStatus.OutcomeUnknown ||
                    (Attempt.Status == PaymentCreationStatus.Requested && Attempt.ExpiresAt > now &&
                        Attempt.Order.OrderStatusId == OrderStatus.Pending.Id)), ct);

        var unresolvedOperations = await context.PaymentOperations.IgnoreQueryFilters()
            .CountAsync(Operation => Operation.EnterpriseId == enterpriseId && Operation.Order.CustomerId == customerId &&
                Operation.Status != PaymentOperationStatus.Succeeded && Operation.Status != PaymentOperationStatus.Superseded, ct);

        if (uncertainPayments > 0 || unresolvedOperations > 0)
            blockers.Add(new AccountDeletionBlocker("AccountDeletion.UnresolvedPaymentOperations",
                "Resolva os cancelamentos e estornos pendentes antes de excluir a conta.", uncertainPayments + unresolvedOperations));

        var activeOrders = await context.Orders.IgnoreQueryFilters()
            .CountAsync(Order => Order.EnterpriseId == enterpriseId && Order.CustomerId == customerId &&
                (Order.OrderStatusId == OrderStatus.Confirmed.Id || Order.OrderStatusId == OrderStatus.Processing.Id ||
                    Order.OrderStatusId == OrderStatus.Shipped.Id), ct);
        var activeShipments = await context.Shipments.IgnoreQueryFilters()
            .CountAsync(Shipment => Shipment.Order.EnterpriseId == enterpriseId && Shipment.Order.CustomerId == customerId &&
                Shipment.ShipmentStatusId != ShipmentStatus.Delivered.Id && Shipment.ShipmentStatusId != ShipmentStatus.Cancelled.Id &&
                Shipment.ShipmentStatusId != ShipmentStatus.Returned.Id, ct);

        if (activeOrders > 0 || activeShipments > 0)
            blockers.Add(new AccountDeletionBlocker("AccountDeletion.OpenOrders",
                "Há pedidos ou envios em andamento. Aguarde a conclusão ou o cancelamento.", Math.Max(activeOrders, activeShipments)));

        var pendingInvoices = await context.Invoices.IgnoreQueryFilters()
            .CountAsync(Invoice => Invoice.EnterpriseId == enterpriseId && Invoice.CustomerId == customerId &&
                Invoice.InvoiceStatusId == InvoiceStatus.Pending.Id, ct);

        if (pendingInvoices > 0)
            blockers.Add(new AccountDeletionBlocker("AccountDeletion.PendingInvoices",
                "Há documentos fiscais pendentes. Aguarde a conclusão ou o cancelamento.", pendingInvoices));

        return new AccountDeletionEligibility(blockers.Count == 0, blockers);
    }

    public async Task EraseCustomerRelatedDataAsync(long enterpriseId, long customerId, CancellationToken ct)
    {
        RequireOperation();
        await context.CustomerRefreshTokens.IgnoreQueryFilters()
            .Where(Token => Token.EnterpriseId == enterpriseId && Token.CustomerId == customerId).ExecuteDeleteAsync(ct);
        await context.CustomerActionTokens.IgnoreQueryFilters()
            .Where(Token => Token.EnterpriseId == enterpriseId && Token.CustomerId == customerId).ExecuteDeleteAsync(ct);
        await context.CartItems.IgnoreQueryFilters()
            .Where(Item => Item.EnterpriseId == enterpriseId && Item.Cart.CustomerId == customerId).ExecuteDeleteAsync(ct);
        await context.Carts.IgnoreQueryFilters()
            .Where(Cart => Cart.EnterpriseId == enterpriseId && Cart.CustomerId == customerId).ExecuteDeleteAsync(ct);
        await context.CustomerAddresses.IgnoreQueryFilters()
            .Where(Address => Address.EnterpriseId == enterpriseId && Address.CustomerId == customerId).ExecuteDeleteAsync(ct);

        // Documentos de cadastro, não notas fiscais ou snapshots comerciais. ReferenceType legado é polimórfico.
        await context.Documents.IgnoreQueryFilters()
            .Where(Document => Document.EnterpriseId == enterpriseId &&
                (EF.Property<long?>(Document, "CustomerId") == customerId ||
                    (Document.ReferenceId == customerId &&
                        (Document.ReferenceType.ToLower() == "customer" || Document.ReferenceType.ToLower() == "cliente" ||
                            Document.ReferenceType == "StoreCommerce.Domain.Entity.Customer"))))
            .ExecuteDeleteAsync(ct);

        // O pedido já tem seu snapshot. A cópia redundante para criar a sessão deixa de ser necessária.
        await context.PaymentAttempts.IgnoreQueryFilters()
            .Where(Attempt => Attempt.EnterpriseId == enterpriseId && Attempt.CustomerId == customerId)
            .ExecuteUpdateAsync(Setters => Setters.SetProperty(Attempt => Attempt.CustomerName, string.Empty)
                .SetProperty(Attempt => Attempt.CustomerEmail, string.Empty), ct);
    }

    public async Task EraseUserRelatedDataAsync(long userId, string normalizedEmail, DateTime now, CancellationToken ct)
    {
        RequireOperation();
        var enterprises = await context.EnterpriseMemberships.IgnoreQueryFilters().Where(Membership => Membership.UserId == userId)
            .Select(Membership => Membership.EnterpriseId).Distinct().OrderBy(Id => Id).ToListAsync(ct);

        foreach (var enterpriseId in enterprises)
            await context.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {enterpriseId} FOR UPDATE", ct);

        await context.UserRefreshTokens.Where(Token => Token.UserId == userId).ExecuteDeleteAsync(ct);
        await context.UserActionTokens.Where(Token => Token.UserId == userId).ExecuteDeleteAsync(ct);
        await context.Employees.IgnoreQueryFilters().Where(Employee => Employee.EnterpriseMembership.UserId == userId)
            .ExecuteUpdateAsync(Setters => Setters.SetProperty(Employee => Employee.FullName, "Funcionário excluído")
                .SetProperty(Employee => Employee.IsActive, false), ct);
        await context.MembershipAccessRoles.IgnoreQueryFilters()
            .Where(Role => Role.EnterpriseMembership.UserId == userId).ExecuteDeleteAsync(ct);
        await context.OwnershipTransfers.IgnoreQueryFilters()
            .Where(Transfer => Transfer.IssuedByMembership.UserId == userId || Transfer.RecipientMembership.UserId == userId)
            .ExecuteUpdateAsync(Setters => Setters.SetProperty(Transfer => Transfer.RevokedAt, now), ct);

        // O e-mail é utilizado apenas para convites endereçados à conta, nunca para excluir outras identidades.
        var erasedEmail = $"deleted-{Guid.NewGuid():N}@deleted.invalid".ToUpperInvariant();
        await context.EnterpriseInvitations.IgnoreQueryFilters()
            .Where(Invitation => Invitation.InvitedNormalizedEmail == normalizedEmail)
            .ExecuteUpdateAsync(Setters => Setters.SetProperty(Invitation => Invitation.InvitedNormalizedEmail, erasedEmail)
                .SetProperty(Invitation => Invitation.RevokedAt, now), ct);
        await context.EnterpriseInvitations.IgnoreQueryFilters()
            .Where(Invitation => Invitation.IssuedByMembership.UserId == userId && Invitation.AcceptedAt == null)
            .ExecuteUpdateAsync(Setters => Setters.SetProperty(Invitation => Invitation.RevokedAt, now), ct);
        await context.EnterpriseMemberships.IgnoreQueryFilters().Where(Membership => Membership.UserId == userId)
            .ExecuteUpdateAsync(Setters => Setters.SetProperty(Membership => Membership.IsActive, false)
                .SetProperty(Membership => Membership.AccessVersion, Membership => Membership.AccessVersion + 1), ct);
    }

    private void RequireOperation()
    {
        if (!scope.IsOpen || context.Database.CurrentTransaction is null)
            throw new InvalidOperationException("A exclusão de conta exige escopo de identidade e transação.");
    }
}
