using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RegisterCustomerCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RegisterCustomerCommand, CustomerResponse>(context)
{
    protected override async Task<CustomerResponse> ExecuteAsync(RegisterCustomerCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var tenant = actor.EnterpriseId;
        var email = NormalizeEmail(request.Email);
        AccountPasswordPolicy.Validate(request.Password);

        if (await data.Customers.ExistsAsync(x => x.EnterpriseId == tenant && x.NormalizedEmail == email, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(409, "Identity.AccountUnavailable", "Não é possível cadastrar esta conta.");

        var customer = Customer.Create(tenant, request.FullName, CustomerEmail.Create(request.Email), CustomerPhone.Create(request.Phone),
            request.AddressLine, request.City, request.StateId, request.ZipCode, request.Country, request.Gender,
            DateTime.SpecifyKind(request.DateOfBirth.Date, DateTimeKind.Utc), false, true);
        customer.SetPassword(passwords.Hash(customer, request.Password), false);
        using var operation = scope.Begin();
        data.Customers.Add(customer);
        await unitOfWork.CommitAsync(ct);

        return CustomerResponse.FromEntity(customer);
    }
}
