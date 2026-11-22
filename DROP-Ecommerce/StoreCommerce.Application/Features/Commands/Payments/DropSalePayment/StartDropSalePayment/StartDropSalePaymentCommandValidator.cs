using FluentValidation;
using FluentValidation.Results;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StartDropSalePaymentCommandValidator : AbstractValidator<StartDropSalePaymentCommand>
{
    public StartDropSalePaymentCommandValidator()
    {
        RuleFor(Command => Command).Custom((command, context) =>
        {
            var request = command.Input;

            if (request is null)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe os dados da operação.") { ErrorCode = "Validation.InputRequired" });

                return;
            }

            if (request.ExpiresAt.Kind != DateTimeKind.Utc)
            {
                context.AddFailure(new ValidationFailure("Input.ExpiresAt", "O prazo da cobrança deve estar em UTC.") { ErrorCode = "DropPayment.InvalidDeadline" });

                return;
            }

            if (request.OperationId == Guid.Empty || request.DropOrderId < 1 || request.CustomerId < 1 || request.Amount <= 0 ||
                request.Amount > 9999999999999999.99m || decimal.Round(request.Amount, 2) != request.Amount || request.Currency != "BRL")
            {
                context.AddFailure(new ValidationFailure("Input", "Intenção de pagamento inválida.") { ErrorCode = "DropPayment.InvalidIntent" });

                return;
            }

        });
    }
}
