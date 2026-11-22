using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropTransactionCommandValidator : AbstractValidator<CreateListDropTransactionCommand>
{
    public CreateListDropTransactionCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropTransactionCommandValidator());
    }
}
