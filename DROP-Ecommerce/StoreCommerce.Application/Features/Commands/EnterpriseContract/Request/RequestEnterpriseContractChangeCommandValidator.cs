using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RequestEnterpriseContractChangeCommandValidator
    : AbstractValidator<RequestEnterpriseContractChangeCommand>
{
    public RequestEnterpriseContractChangeCommandValidator()
    {
        RuleFor(Command => Command.Request).NotNull();
        When(Command => Command.Request is not null, () =>
        {
            RuleFor(Command => Command.Request.IdempotencyKey).NotEmpty().MaximumLength(100);
            RuleFor(Command => Command.Request.PlanId).GreaterThan(0);
            RuleFor(Command => Command.Request.AddOnId).GreaterThan(0)
                .When(Command => Command.Request.AddOnId.HasValue);
            RuleFor(Command => Command.Request.ExpectedContractRevision).GreaterThan(0);
            RuleFor(Command => Command.Request.CatalogVersion).GreaterThan(0);
        });
    }
}
