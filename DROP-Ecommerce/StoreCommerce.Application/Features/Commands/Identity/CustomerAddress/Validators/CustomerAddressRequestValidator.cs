using FluentValidation;
using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CustomerAddressRequestValidator : AbstractValidator<CustomerAddressRequest>
{
    public CustomerAddressRequestValidator()
    {
        RuleFor(request => request.Label).MaximumLength(CustomerAddress.LabelMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.RecipientName).MaximumLength(CustomerAddress.RecipientNameMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.AddressLine)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(CustomerAddress.AddressLineMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.City)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(CustomerAddress.CityMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.StateId)
            .Must(stateId => BaseStaticEntity.GetAll<State>().Any(state => state.Id == stateId)).WithMessage("{PropertyName} inválido.");
        RuleFor(request => request.ZipCode)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(CustomerAddress.ZipCodeMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.Country).MaximumLength(CustomerAddress.CountryMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}
