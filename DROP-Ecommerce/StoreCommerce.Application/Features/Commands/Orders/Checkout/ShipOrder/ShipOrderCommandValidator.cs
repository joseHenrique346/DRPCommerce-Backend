using FluentValidation;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Request).NotNull();

        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.ShipmentTypeId)
                .Must(typeId => BaseStaticEntity.GetAll<ShipmentType>().Any(type => type.Id == typeId))
                .When(command => command.Request.ShipmentTypeId.HasValue).WithName("shipmentTypeId").WithMessage("{PropertyName} inválido.");

            RuleFor(command => command.Request.ShipmentTypeId)
                .NotNull().When(command => command.Request.EstimatedDelivery.HasValue || !string.IsNullOrWhiteSpace(command.Request.CarrierName) ||
                    !string.IsNullOrWhiteSpace(command.Request.TrackingCode))
                .WithName("shipmentTypeId").WithMessage("Informe o tipo de envio.");

            RuleFor(command => command.Request.EstimatedDelivery)
                .NotNull().When(command => command.Request.ShipmentTypeId.HasValue)
                .WithName("estimatedDelivery").WithMessage("Informe a previsão de entrega.");

            RuleFor(command => command.Request.CarrierName)
                .NotEmpty().When(command => command.Request.ShipmentTypeId.HasValue && command.Request.ShipmentTypeId != ShipmentType.Pickup.Id)
                .WithName("carrierName").WithMessage("Informe a transportadora.")
                .MaximumLength(Shipment.CarrierNameMaxLength).WithName("carrierName").WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

            RuleFor(command => command.Request.TrackingCode)
                .MaximumLength(Shipment.TrackingCodeMaxLength).WithName("trackingCode").WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

            RuleFor(command => command.Request.Note)
                .MaximumLength(OrderStatusHistory.ReasonMaxLength).WithName("note").WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        });
    }
}
