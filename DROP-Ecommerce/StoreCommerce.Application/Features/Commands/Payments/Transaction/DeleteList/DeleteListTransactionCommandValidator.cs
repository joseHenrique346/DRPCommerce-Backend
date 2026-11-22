using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListTransactionCommandValidator : AbstractValidator<DeleteListTransactionCommand>
{
    public DeleteListTransactionCommandValidator()
    {
        RuleFor(transaction => transaction.ids).NotEmpty();
        RuleForEach(transaction => transaction.ids).GreaterThan(0);
    }
}
