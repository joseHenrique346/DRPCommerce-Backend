using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEnterpriseDomainCommandValidator : AbstractValidator<CreateEnterpriseDomainCommand>
{
    public CreateEnterpriseDomainCommandValidator()
    {
        RuleFor(command => command.enterpriseId).GreaterThan(0);
        RuleFor(command => command.host).NotEmpty().MaximumLength(253);
    }
}
