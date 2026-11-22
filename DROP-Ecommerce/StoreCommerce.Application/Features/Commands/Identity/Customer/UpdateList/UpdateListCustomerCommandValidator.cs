using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListCustomerCommandValidator : AbstractValidator<UpdateListCustomerCommand>
{
    public UpdateListCustomerCommandValidator()
    {
        RuleFor(customer => customer.commands).NotEmpty();
        RuleForEach(customer => customer.commands).SetValidator(new UpdateCustomerCommandValidator());
    }
}
