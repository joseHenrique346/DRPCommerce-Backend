using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(d => d.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.referenceId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.referenceType)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(d => d.typeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.number)
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(d => d.fileUrl)
            .MaximumLength(1000).WithMessage("{PropertyName} não pode ter mais de 1000 caracteres.");

        When(d => !string.IsNullOrEmpty(d.fileUrl), () =>
        {
            RuleFor(d => d.fileUrl)
                .Must(uri => Uri.TryCreate(uri!, UriKind.Absolute, out _))
                .WithMessage("{PropertyName} tem formato de URL inválido.");
        });

        RuleFor(d => d.statusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.issuedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("{PropertyName} não pode estar no futuro.");

        RuleFor(d => d.expiresAt)
            .GreaterThan(d => d.issuedAt).WithMessage("{PropertyName} deve ser posterior a IssuedAt.")
            .When(d => d.expiresAt.HasValue);
    }
}
