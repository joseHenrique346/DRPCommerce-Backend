using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEnterpriseDomainCommandValidator : AbstractValidator<CreateListEnterpriseDomainCommand>
{
    public CreateListEnterpriseDomainCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new CreateEnterpriseDomainCommandValidator());
    }
}
