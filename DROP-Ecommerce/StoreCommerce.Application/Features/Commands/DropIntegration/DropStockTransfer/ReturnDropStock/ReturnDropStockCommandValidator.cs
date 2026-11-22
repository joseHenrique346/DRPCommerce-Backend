using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReturnDropStockCommandValidator : AbstractValidator<ReturnDropStockCommand>
{
    public ReturnDropStockCommandValidator()
    {
        RuleFor(Request => Request.Input).NotNull();
    }
}
