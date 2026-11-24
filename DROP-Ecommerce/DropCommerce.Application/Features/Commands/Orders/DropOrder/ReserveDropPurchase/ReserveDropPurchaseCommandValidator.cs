using FluentValidation;
using FluentValidation.Results;

namespace DropCommerce.Application.Features.Commands;

public sealed class ReserveDropPurchaseCommandValidator : AbstractValidator<ReserveDropPurchaseCommand>
{
    public ReserveDropPurchaseCommandValidator()
    {
        RuleFor(Command => Command).Custom((command, context) =>
        {
            var request = command.Input;

            if (request is null)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe os dados da operação.") { ErrorCode = "Validation.InputRequired" });

                return;
            }

            if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Length > 100 ||
                request.Lines is null || request.Lines.Count is < 1 or > 200 || request.Lines.Any(Line => Line is null || Line.Quantity is < 1 or > 1000000) ||
                request.Lines.Select(Line => Line.VariantId).Distinct().Count() != request.Lines.Count)
            {
                context.AddFailure(new ValidationFailure("Input", "Contrato ou reserva inválida.") { ErrorCode = "DropPurchase.InvalidReservation" });

                return;
            }

        });
    }
}
