using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeactivateEmployeeCommandHandler(IdentityHandlerContext context, IRepository<Employee> employees)
    : IdentityHandlerBase<DeactivateEmployeeCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(DeactivateEmployeeCommand command, CancellationToken ct)
    {
        using var operation = scope.Begin();
        await using var transaction = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var employee = await employees.FindAsync(
            Employee => Employee.Id == command.Id && Employee.EnterpriseId == tenant,
            ct)
            ?? throw NotFound();
        var membership = await data.EnterpriseMemberships.FindAsync(
            Membership => Membership.Id == employee.EnterpriseMembershipId && Membership.EnterpriseId == tenant,
            ct,
            ignoreQueryFilters: true)
            ?? throw NotFound();

        if (membership.KindId == EnterpriseMembershipKind.PrimaryOwner.Id)
            throw new SecurityFailure(409, "Access.PrimaryOwnerProtected", "O dono principal não pode ser removido, rebaixado ou desativado.");

        employee.Deactivate();
        membership.Deactivate();

        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}
