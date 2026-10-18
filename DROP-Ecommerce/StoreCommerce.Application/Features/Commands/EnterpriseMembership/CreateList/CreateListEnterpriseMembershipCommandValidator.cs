using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEnterpriseMembershipCommandValidator : AbstractValidator<CreateListEnterpriseMembershipCommand>
{
    public CreateListEnterpriseMembershipCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new CreateEnterpriseMembershipCommandValidator());
    }
}
