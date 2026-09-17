using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListInvoiceCommandValidator : AbstractValidator<DeleteListInvoiceCommand>
{
    public DeleteListInvoiceCommandValidator()
    {
        RuleFor(invoice => invoice.ids).NotEmpty();
        RuleForEach(invoice => invoice.ids).GreaterThan(0);
    }
}
