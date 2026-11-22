using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListDocumentCommandValidator : AbstractValidator<DeleteListDocumentCommand>
{
    public DeleteListDocumentCommandValidator()
    {
        RuleFor(document => document.ids).NotEmpty();
        RuleForEach(document => document.ids).GreaterThan(0);
    }
}
