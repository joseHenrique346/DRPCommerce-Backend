using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropTransactionCommandValidator : AbstractValidator<DeleteListDropTransactionCommand>
{
    public DeleteListDropTransactionCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
