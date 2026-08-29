using FluentValidation;

namespace StoreCommerce.Domain.Entity.Coupon;

internal class CouponValidator : AbstractValidator<Coupon>
{
    public CouponValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(c => c.UpdatedAt)
            .NotEqual(default(DateTime)).When(c => c.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(c => c.EnterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(c => c.TypeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.DiscountValue)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.MinOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.MaxDiscountCap)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.MaxUses)
            .GreaterThan(0).When(c => c.MaxUses.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(c => c.UsedCount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.StartsAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(c => c.ExpiresAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.")
            .GreaterThan(c => c.StartsAt).WithMessage("{PropertyName} deve ser posterior a StartsAt.");
    }
}
