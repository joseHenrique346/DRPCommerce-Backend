using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(c => c.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.code)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .Length(3, 50).WithMessage("{PropertyName} deve ter entre 3 e 50 caracteres.");

        RuleFor(c => c.typeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.discountValue)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.minOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.maxDiscountCap)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.maxUses)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero quando fornecido.")
            .When(c => c.maxUses.HasValue);

        RuleFor(c => c.usedCount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.expiresAt)
            .GreaterThan(c => c.startsAt).WithMessage("{PropertyName} deve ser posterior a StartsAt.");
    }
}
