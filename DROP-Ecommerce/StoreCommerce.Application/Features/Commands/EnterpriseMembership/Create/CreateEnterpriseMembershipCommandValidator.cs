using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEnterpriseMembershipCommandValidator : AbstractValidator<CreateEnterpriseMembershipCommand>
{
    public CreateEnterpriseMembershipCommandValidator()
    {
        RuleFor(command => command.userId).GreaterThan(0);
        RuleFor(command => command.kindId).InclusiveBetween(1L, 3L);
        RuleFor(command => command.joinedAt).NotEqual(default(DateTime));
    }
}
