using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListDocumentCommandValidator : AbstractValidator<CreateListDocumentCommand>
{
    public CreateListDocumentCommandValidator()
    {
        RuleFor(document => document.commands).NotEmpty();
        RuleForEach(document => document.commands).SetValidator(new CreateDocumentCommandValidator());
    }
}
