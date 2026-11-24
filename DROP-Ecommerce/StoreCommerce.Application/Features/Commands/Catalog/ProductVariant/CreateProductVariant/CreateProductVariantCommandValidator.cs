using FluentValidation;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(Command => Command.ProductId).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull();
        When(Command => Command.Request is not null, () =>
        {
            RuleFor(Command => Command.Request.Details).NotNull().SetValidator(new VariantDetailsRequestValidator());
        });
    }
}
