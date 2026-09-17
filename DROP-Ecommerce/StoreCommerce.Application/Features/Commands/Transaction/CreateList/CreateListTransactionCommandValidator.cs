using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListTransactionCommandValidator : AbstractValidator<CreateListTransactionCommand>
{
    public CreateListTransactionCommandValidator()
    {
        RuleFor(transaction => transaction.commands).NotEmpty();
        RuleForEach(transaction => transaction.commands).SetValidator(new CreateTransactionCommandValidator());
    }
}
