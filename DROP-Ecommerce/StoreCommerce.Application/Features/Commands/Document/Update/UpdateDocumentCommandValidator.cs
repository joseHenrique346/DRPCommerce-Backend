using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(document => document.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(document => document.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(document => document.referenceId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(document => document.referenceType)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(document => document.typeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(document => document.number)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(document => document.fileUrl)
            .MaximumLength(500).When(document => !string.IsNullOrWhiteSpace(document.fileUrl))
            .WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(document => document.statusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(document => document.issuedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");
    }
}