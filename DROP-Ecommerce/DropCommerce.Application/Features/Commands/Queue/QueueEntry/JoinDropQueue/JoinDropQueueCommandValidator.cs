using FluentValidation;
using FluentValidation.Results;
using DropCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Features.Commands;

public sealed class JoinDropQueueCommandValidator : AbstractValidator<JoinDropQueueCommand>
{
    public JoinDropQueueCommandValidator()
    {
        RuleFor(Command => Command).Custom((command, context) =>
        {
            var request = command.Input;

            if (request is null)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe os dados da operação.") { ErrorCode = "Validation.InputRequired" });

                return;
            }

            try
            {
                DropPurchaseIdentity.NormalizeCpf(request.CPF);
            }
            catch (SecurityFailure failure)
            {
                context.AddFailure(new ValidationFailure("Input.CPF", failure.Message) { ErrorCode = failure.Code });
            }
        });
    }
}
