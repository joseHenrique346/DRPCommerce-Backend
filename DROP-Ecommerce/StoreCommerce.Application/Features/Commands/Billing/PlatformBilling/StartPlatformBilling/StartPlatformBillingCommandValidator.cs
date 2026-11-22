using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StartPlatformBillingCommandValidator : AbstractValidator<StartPlatformBillingCommand>
{
    public StartPlatformBillingCommandValidator()
    {
        RuleFor(Request => Request.Input).NotNull();
    }
}
