using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(document => document.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}