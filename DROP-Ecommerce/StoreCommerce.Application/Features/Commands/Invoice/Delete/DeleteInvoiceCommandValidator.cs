using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteInvoiceCommandValidator : AbstractValidator<DeleteInvoiceCommand>
{
    public DeleteInvoiceCommandValidator()
    {
        RuleFor(invoice => invoice.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}