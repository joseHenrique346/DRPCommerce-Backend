using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(service => service.categoryId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(service => service.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(Service.NameMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(service => service.description)
            .MaximumLength(Service.DescriptionMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(service => service.price)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(service => service.durationMinutes)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
