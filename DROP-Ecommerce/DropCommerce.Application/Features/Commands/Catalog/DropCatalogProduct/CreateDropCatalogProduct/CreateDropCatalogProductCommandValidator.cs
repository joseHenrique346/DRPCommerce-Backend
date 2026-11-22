using FluentValidation;
using FluentValidation.Results;

namespace DropCommerce.Application.Features.Commands;

public sealed class CreateDropCatalogProductCommandValidator : AbstractValidator<CreateDropCatalogProductCommand>
{
    public CreateDropCatalogProductCommandValidator()
    {
        RuleFor(Command => Command).Custom((command, context) =>
        {
            var request = command.Input;

            if (request is null)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe os dados da operação.") { ErrorCode = "Validation.InputRequired" });

                return;
            }

            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 255 || request.Description is null || request.Description.Length > 10000 ||
                request.Variants is null || request.Variants.Count is < 1 or > 200 ||
                request.Variants.Any(Variant => Variant is null || string.IsNullOrWhiteSpace(Variant.SKU) || Variant.SKU.Length > 100 ||
                Variant.Price <= 0 || Variant.Price > 9999999999.99m || decimal.Round(Variant.Price, 2) != Variant.Price || Variant.Quantity is < 0 or > 1000000) ||
                request.Variants.Select(Variant => Variant.SKU.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != request.Variants.Count)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe de uma a duzentas variações com SKUs distintos.") { ErrorCode = "DropCatalog.InvalidVariants" });

                return;
            }

            foreach (var variant in request.Variants)
            {
                if (variant.Options is null || variant.Options.Count > 20 || variant.Options.Any(Option =>
                    string.IsNullOrWhiteSpace(Option.Key) || Option.Key.Length > 80 || string.IsNullOrWhiteSpace(Option.Value) || Option.Value.Length > 150))
                {
                    context.AddFailure(new ValidationFailure("Input", "Opções de variação inválidas.") { ErrorCode = "DropCatalog.InvalidOptions" });

                    return;
                }
            }
        });
    }
}
