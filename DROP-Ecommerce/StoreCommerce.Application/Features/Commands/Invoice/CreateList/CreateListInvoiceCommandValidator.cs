using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListInvoiceCommandValidator : AbstractValidator<CreateListInvoiceCommand>
{
    public CreateListInvoiceCommandValidator()
    {
        RuleFor(invoice => invoice.commands).NotEmpty();
        RuleForEach(invoice => invoice.commands).SetValidator(new CreateInvoiceCommandValidator());
    }
}
