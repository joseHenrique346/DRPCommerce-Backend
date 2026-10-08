using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEnterpriseDomainCommandValidator : AbstractValidator<DeleteListEnterpriseDomainCommand>
{
    public DeleteListEnterpriseDomainCommandValidator()
    {
        RuleFor(command => command.ids).NotEmpty();
        RuleForEach(command => command.ids).GreaterThan(0);
    }
}
