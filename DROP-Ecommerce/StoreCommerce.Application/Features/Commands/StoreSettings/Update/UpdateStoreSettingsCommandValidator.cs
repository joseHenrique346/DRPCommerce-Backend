using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateStoreSettingsCommandValidator : AbstractValidator<UpdateStoreSettingsCommand>
{
    public UpdateStoreSettingsCommandValidator()
    {
        RuleFor(settings => settings.currencyCode)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .Matches("^[A-Za-z]{3}$").WithMessage("{PropertyName} deve ser um código ISO 4217 de 3 letras (ex.: BRL).");

        RuleFor(settings => settings.minOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(settings => settings.maxItemsPerOrder)
            .InclusiveBetween(1, StoreSettings.MaxItemsPerOrderLimit).When(settings => settings.maxItemsPerOrder.HasValue)
            .WithMessage("{PropertyName} deve estar entre {From} e {To} quando fornecido.");

        RuleFor(settings => settings.pendingOrderExpirationMinutes)
            .InclusiveBetween(StoreSettings.MinPendingOrderExpirationMinutes, StoreSettings.MaxPendingOrderExpirationMinutes)
            .WithMessage("{PropertyName} deve estar entre {From} e {To} minutos.");
    }
}
