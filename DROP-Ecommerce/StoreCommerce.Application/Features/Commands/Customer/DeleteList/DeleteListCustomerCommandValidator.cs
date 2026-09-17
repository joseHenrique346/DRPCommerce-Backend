using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListCustomerCommandValidator : AbstractValidator<DeleteListCustomerCommand>
{
    public DeleteListCustomerCommandValidator()
    {
        RuleFor(customer => customer.ids).NotEmpty();
        RuleForEach(customer => customer.ids).GreaterThan(0);
    }
}
