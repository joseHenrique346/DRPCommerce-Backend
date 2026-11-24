using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEnterpriseDomainCommandValidator : AbstractValidator<UpdateListEnterpriseDomainCommand>
{
    public UpdateListEnterpriseDomainCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new UpdateEnterpriseDomainCommandValidator());
    }
}
