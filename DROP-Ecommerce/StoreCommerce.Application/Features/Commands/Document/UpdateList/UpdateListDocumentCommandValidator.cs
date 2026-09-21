using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListDocumentCommandValidator : AbstractValidator<UpdateListDocumentCommand>
{
    public UpdateListDocumentCommandValidator()
    {
        RuleFor(document => document.commands).NotEmpty();
        RuleForEach(document => document.commands).SetValidator(new UpdateDocumentCommandValidator());
    }
}
