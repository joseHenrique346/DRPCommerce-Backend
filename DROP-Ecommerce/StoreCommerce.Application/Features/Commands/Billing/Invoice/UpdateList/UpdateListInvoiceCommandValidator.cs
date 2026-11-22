using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListInvoiceCommandValidator : AbstractValidator<UpdateListInvoiceCommand>
{
    public UpdateListInvoiceCommandValidator()
    {
        RuleFor(invoice => invoice.commands).NotEmpty();
        RuleForEach(invoice => invoice.commands).SetValidator(new UpdateInvoiceCommandValidator());
    }
}
