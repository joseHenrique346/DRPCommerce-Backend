using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateInvoiceCommandValidator : AbstractValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceCommandValidator()
    {
        RuleFor(invoice => invoice.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(invoice => invoice.orderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(invoice => invoice.customerId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(invoice => invoice.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(invoice => invoice.number)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(invoice => invoice.series)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(invoice => invoice.accessKey)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(invoice => invoice.typeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(invoice => invoice.statusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(invoice => invoice.totalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(invoice => invoice.taxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(invoice => invoice.fileUrl)
            .MaximumLength(500).When(invoice => !string.IsNullOrWhiteSpace(invoice.fileUrl))
            .WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");
    }
}