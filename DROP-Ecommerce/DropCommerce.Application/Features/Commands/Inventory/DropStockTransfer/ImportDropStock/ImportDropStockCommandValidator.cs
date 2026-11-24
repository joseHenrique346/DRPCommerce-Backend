using FluentValidation;
using FluentValidation.Results;
using System.Text.Json;

namespace DropCommerce.Application.Features.Commands;

public sealed class ImportDropStockCommandValidator : AbstractValidator<ImportDropStockCommand>
{
    public ImportDropStockCommandValidator()
    {
        RuleFor(Command => Command).Custom((command, context) =>
        {
            var request = command.Export;

            if (request is null)
            {
                context.AddFailure(new ValidationFailure("Input", "Informe os dados da operação.") { ErrorCode = "Validation.InputRequired" });

                return;
            }

            if (request.EnterpriseId < 1 || request.ProductId < 1 || request.OperationId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 255 || request.Description is null || request.Description.Length > 10000 ||
                request.Variants is null || request.Variants.Count is < 1 or > 200 ||
                request.Variants.Any(Variant => Variant is null || Variant.SourceVariantId < 1 || string.IsNullOrWhiteSpace(Variant.SKU) || Variant.SKU.Length > 100 ||
                Variant.Quantity is < 0 or > 1000000 || Variant.Price <= 0 || Variant.Price > 9999999999.99m) ||
                request.Variants.Select(Variant => Variant.SourceVariantId).Distinct().Count() != request.Variants.Count)
            {
                context.AddFailure(new ValidationFailure("Input", "A transferência de origem é inválida.") { ErrorCode = "DropStock.InvalidExport" });

                return;
            }
            foreach (var variant in request.Variants)
            {
                try
                {
                    using var json = JsonDocument.Parse(variant.OptionsJson);

                    if (json.RootElement.ValueKind != JsonValueKind.Object)
                        context.AddFailure(new ValidationFailure("Input", "As opções do SKU de origem são inválidas.") { ErrorCode = "DropStock.InvalidOptions" });
                }
                catch (Exception exception) when (exception is JsonException or ArgumentNullException)
                {
                    context.AddFailure(new ValidationFailure("Input", "As opções do SKU de origem são inválidas.") { ErrorCode = "DropStock.InvalidOptions" });
                }
            }
        });
    }
}
