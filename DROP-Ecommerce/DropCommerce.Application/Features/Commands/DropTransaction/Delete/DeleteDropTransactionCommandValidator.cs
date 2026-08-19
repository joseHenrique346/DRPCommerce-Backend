using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropTransactionCommandValidator : AbstractValidator<DeleteDropTransactionCommand>
{
    public DeleteDropTransactionCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}