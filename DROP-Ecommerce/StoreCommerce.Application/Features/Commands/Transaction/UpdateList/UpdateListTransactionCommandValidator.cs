using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListTransactionCommandValidator : AbstractValidator<UpdateListTransactionCommand>
{
    public UpdateListTransactionCommandValidator()
    {
        RuleFor(transaction => transaction.commands).NotEmpty();
        RuleForEach(transaction => transaction.commands).SetValidator(new UpdateTransactionCommandValidator());
    }
}
