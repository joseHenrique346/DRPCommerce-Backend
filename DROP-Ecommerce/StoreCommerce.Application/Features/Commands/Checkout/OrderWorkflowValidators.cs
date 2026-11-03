using FluentValidation;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(command => command.Request).NotNull().SetValidator(new CheckoutRequestValidator());
    }
}

public sealed class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(request => request.IdempotencyKey)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(Order.IdempotencyKeyMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("{PropertyName} aceita apenas letras, números, hífen e sublinhado.");

        RuleFor(request => request.PaymentMethodId)
            .Must(methodId => BaseStaticEntity.GetAll<TransactionMethod>().Any(method => method.Id == methodId))
            .WithMessage("{PropertyName} inválido.");

        RuleFor(request => request.CouponCode)
            .MaximumLength(Coupon.CodeMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(request => request.Notes)
            .MaximumLength(Order.NotesMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(request => request.AddressId).GreaterThan(0).When(request => request.AddressId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(request => request.UseProfileAddress).Equal(false).When(request => request.AddressId.HasValue)
            .WithMessage("Informe addressId ou useProfileAddress, não os dois.");

        When(request => !request.UseProfileAddress && !request.AddressId.HasValue, () =>
        {
            RuleFor(request => request.ShippingAddressLine)
                .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
                .MaximumLength(Order.ShippingAddressLineMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

            RuleFor(request => request.ShippingCity)
                .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
                .MaximumLength(Order.ShippingCityMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

            RuleFor(request => request.ShippingStateId)
                .NotNull().WithMessage("{PropertyName} não pode ser vazio.")
                .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

            RuleFor(request => request.ShippingZipCode)
                .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
                .MaximumLength(Order.ShippingZipCodeMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        });
    }
}

public sealed class GetMyOrdersQueryValidator : AbstractValidator<GetMyOrdersQuery>
{
    public GetMyOrdersQueryValidator()
    {
        RuleFor(query => query.Page).InclusiveBetween(1, 100000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class CancelMyOrderCommandValidator : AbstractValidator<CancelMyOrderCommand>
{
    public CancelMyOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Reason).MaximumLength(OrderStatusHistory.ReasonMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

public sealed class ConfirmOrderPaymentCommandValidator : AbstractValidator<ConfirmOrderPaymentCommand>
{
    public ConfirmOrderPaymentCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.TransactionMethodId)
            .Must(methodId => BaseStaticEntity.GetAll<TransactionMethod>().Any(method => method.Id == methodId))
            .When(command => command.TransactionMethodId.HasValue).WithMessage("{PropertyName} inválido.");
        RuleFor(command => command.GatewayReference).MaximumLength(Transaction.GatewayReferenceMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(command => command.Note).MaximumLength(OrderStatusHistory.ReasonMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

public sealed class AdvanceOrderCommandValidator : AbstractValidator<AdvanceOrderCommand>
{
    public AdvanceOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Step).IsInEnum();
        RuleFor(command => command.Note).MaximumLength(OrderStatusHistory.ReasonMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

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

public sealed class SearchOrdersQueryValidator : AbstractValidator<SearchOrdersQuery>
{
    private static readonly string[] Sorts = ["newest", "oldest", "total_desc", "total_asc"];

    public SearchOrdersQueryValidator()
    {
        RuleFor(query => query.Number).GreaterThan(0).When(query => query.Number.HasValue);
        RuleForEach(query => query.StatusIds)
            .Must(statusId => BaseStaticEntity.GetAll<OrderStatus>().Any(status => status.Id == statusId)).WithMessage("{PropertyName} inválido.");
        RuleForEach(query => query.PaymentStatusIds)
            .Must(statusId => BaseStaticEntity.GetAll<OrderPaymentStatus>().Any(status => status.Id == statusId)).WithMessage("{PropertyName} inválido.");
        RuleFor(query => query.StatusIds).Must(ids => ids is null || ids.Count <= 10).WithMessage("Informe no máximo 10 status.");
        RuleFor(query => query.PaymentStatusIds).Must(ids => ids is null || ids.Count <= 10).WithMessage("Informe no máximo 10 status de pagamento.");
        RuleFor(query => query.CustomerId).GreaterThan(0).When(query => query.CustomerId.HasValue);
        RuleFor(query => query.CouponId).GreaterThan(0).When(query => query.CouponId.HasValue);
        RuleFor(query => query.CreatedTo).GreaterThanOrEqualTo(query => query.CreatedFrom)
            .When(query => query.CreatedFrom.HasValue && query.CreatedTo.HasValue).WithMessage("createdTo deve ser posterior a createdFrom.");
        RuleFor(query => query.MinTotal).GreaterThanOrEqualTo(0).When(query => query.MinTotal.HasValue);
        RuleFor(query => query.MaxTotal).GreaterThanOrEqualTo(query => query.MinTotal ?? 0).When(query => query.MaxTotal.HasValue)
            .WithMessage("maxTotal deve ser maior ou igual a minTotal.");
        RuleFor(query => query.Sort).Must(sort => sort is null || Sorts.Contains(sort))
            .WithMessage("sort deve ser newest, oldest, total_desc ou total_asc.");
        RuleFor(query => query.Page).InclusiveBetween(1, 100000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Reason)
            .NotEmpty().WithMessage("Informe o motivo do cancelamento.")
            .MaximumLength(OrderStatusHistory.ReasonMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

public sealed class ApplyPaymentResultCommandValidator : AbstractValidator<ApplyPaymentResultCommand>
{
    public ApplyPaymentResultCommandValidator()
    {
        RuleFor(command => command.Provider).NotEmpty().MaximumLength(Transaction.GatewayProviderMaxLength);
        RuleFor(command => command.Reference).NotEmpty().MaximumLength(Transaction.GatewayReferenceMaxLength);
        RuleFor(command => command.Status).IsInEnum();
        RuleFor(command => command.PaidAmount).GreaterThanOrEqualTo(0).When(command => command.PaidAmount.HasValue);
    }
}
