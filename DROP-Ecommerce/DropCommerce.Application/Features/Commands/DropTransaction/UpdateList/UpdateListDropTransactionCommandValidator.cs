using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropTransactionCommandValidator : AbstractValidator<UpdateListDropTransactionCommand>
{
    public UpdateListDropTransactionCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropTransactionCommandValidator());
    }
}
