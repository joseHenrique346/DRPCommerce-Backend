using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(customer => customer.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(customer => customer.fullName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(Customer.FullNameMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(customer => customer.email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(customer => customer.phone).NotEmpty().MaximumLength(20);

        RuleFor(customer => customer.addressLine)
            .MaximumLength(Customer.AddressLineMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(customer => customer.city)
            .MaximumLength(Customer.CityMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(customer => customer.stateId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(customer => customer.zipCode)
            .MaximumLength(Customer.ZipCodeMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(customer => customer.country)
            .MaximumLength(Customer.CountryMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(customer => customer.gender)
            .MaximumLength(Customer.GenderMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(customer => customer.dateOfBirth)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");
    }
}
