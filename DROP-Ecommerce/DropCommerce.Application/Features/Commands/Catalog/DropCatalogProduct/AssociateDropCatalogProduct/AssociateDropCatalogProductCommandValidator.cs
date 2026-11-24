using FluentValidation;
using FluentValidation.Results;

namespace DropCommerce.Application.Features.Commands;

public sealed class AssociateDropCatalogProductCommandValidator : AbstractValidator<AssociateDropCatalogProductCommand>
{
    public AssociateDropCatalogProductCommandValidator()
    {
        RuleFor(Command => Command).Custom((command, context) =>
        {
            var request = command.Input;

            if (request is null)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe os dados da operação.") { ErrorCode = "Validation.InputRequired" });

                return;
            }

            if (request.MaxPerCustomer < 1)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe um limite de compra positivo.") { ErrorCode = "DropCatalog.InvalidAllocation" });

                return;
            }

        });
    }
}
