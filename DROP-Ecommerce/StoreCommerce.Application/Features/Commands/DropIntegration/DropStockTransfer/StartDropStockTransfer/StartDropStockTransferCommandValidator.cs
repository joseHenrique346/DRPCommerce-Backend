using FluentValidation;
using FluentValidation.Results;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StartDropStockTransferCommandValidator : AbstractValidator<StartDropStockTransferCommand>
{
    public StartDropStockTransferCommandValidator()
    {
        RuleFor(Command => Command).Custom((command, context) =>
        {
            var request = command.Input;

            if (request is null)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe os dados da operação.") { ErrorCode = "Validation.InputRequired" });

                return;
            }

            if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Trim().Length > 100)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe uma chave de transferência válida.") { ErrorCode = "DropStock.InvalidKey" });

                return;
            }

        });
    }
}
