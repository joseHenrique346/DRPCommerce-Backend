using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SelectPlatformBillingTeamCommandValidator : AbstractValidator<SelectPlatformBillingTeamCommand>
{
    public SelectPlatformBillingTeamCommandValidator()
    {
        RuleFor(Request => Request.Input).NotNull();
    }
}
