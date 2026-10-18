using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateCustomerProfileCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<UpdateCustomerProfileCommand, CustomerResponse>(context)
{
    protected override async Task<CustomerResponse> ExecuteAsync(UpdateCustomerProfileCommand command, CancellationToken ct)
    {
        var request = command.Request;

        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var customer = (Customer)await CurrentAccountAsync(true, true, ct);
        customer.UpdatePersonalInfo(request.FullName, customer.Email, CustomerPhone.Create(request.Phone), request.Gender,
            DateTime.SpecifyKind(request.DateOfBirth.Date, DateTimeKind.Utc));
        customer.UpdateAddress(request.AddressLine, request.City, request.StateId, request.ZipCode, request.Country);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return CustomerResponse.FromEntity(customer);
    }
}
