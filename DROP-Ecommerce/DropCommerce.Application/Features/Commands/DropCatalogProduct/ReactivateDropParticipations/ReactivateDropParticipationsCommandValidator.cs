using FluentValidation;
using FluentValidation.Results;

namespace DropCommerce.Application.Features.Commands;

public sealed class ReactivateDropParticipationsCommandValidator : AbstractValidator<ReactivateDropParticipationsCommand>
{
    public ReactivateDropParticipationsCommandValidator()
    {
        RuleFor(Command => Command).Custom((command, context) =>
        {
            var request = command.Input;

            if (request is null)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe os dados da operação.") { ErrorCode = "Validation.InputRequired" });

                return;
            }

            if (request.Mode is not ("oldest" or "selected"))
            {
                context.AddFailure(new ValidationFailure("Input", "Informe oldest ou selected para o modo de reativação.") { ErrorCode = "DropContract.InvalidReactivationMode" });

                return;
            }

        });
    }
}
