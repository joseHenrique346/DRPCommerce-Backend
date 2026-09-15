using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionCommandValidator()
    {
        RuleFor(transaction => transaction.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}