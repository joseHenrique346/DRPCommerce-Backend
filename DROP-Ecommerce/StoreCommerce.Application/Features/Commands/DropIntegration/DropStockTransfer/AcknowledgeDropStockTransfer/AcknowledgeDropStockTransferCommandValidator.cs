using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AcknowledgeDropStockTransferCommandValidator : AbstractValidator<AcknowledgeDropStockTransferCommand>
{
    public AcknowledgeDropStockTransferCommandValidator()
    {
        RuleFor(Command => Command.CatalogProductId).GreaterThan(0)
            .WithErrorCode("DropStock.InvalidDestination")
            .WithMessage("Destino inválido.");
    }
}
