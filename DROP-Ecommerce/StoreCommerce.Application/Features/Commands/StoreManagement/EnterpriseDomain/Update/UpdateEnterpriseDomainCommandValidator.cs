using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEnterpriseDomainCommandValidator : AbstractValidator<UpdateEnterpriseDomainCommand>
{
    public UpdateEnterpriseDomainCommandValidator()
    {
        RuleFor(command => command.id).GreaterThan(0);
        RuleFor(command => command.host).NotEmpty().MaximumLength(253);
    }
}
