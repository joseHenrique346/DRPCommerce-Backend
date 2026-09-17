using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCustomerCommandValidator : AbstractValidator<CreateListCustomerCommand>
{
    public CreateListCustomerCommandValidator()
    {
        RuleFor(customer => customer.commands).NotEmpty();
        RuleForEach(customer => customer.commands).SetValidator(new CreateCustomerCommandValidator());
    }
}
