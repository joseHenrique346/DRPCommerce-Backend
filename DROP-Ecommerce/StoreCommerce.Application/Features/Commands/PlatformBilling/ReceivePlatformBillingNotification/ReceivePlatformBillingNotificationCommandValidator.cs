using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReceivePlatformBillingNotificationCommandValidator : AbstractValidator<ReceivePlatformBillingNotificationCommand>
{
    public ReceivePlatformBillingNotificationCommandValidator()
    {
        RuleFor(Command => Command.Topic).Must(Topic => Topic is "payment" or "subscription_preapproval" or "subscription_authorized_payment")
            .WithErrorCode("Billing.InvalidTopic")
            .WithMessage("Tópico de cobrança não suportado.");
    }
}
