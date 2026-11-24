using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEnterpriseMembershipCommandValidator : AbstractValidator<UpdateEnterpriseMembershipCommand>
{
    public UpdateEnterpriseMembershipCommandValidator()
    {
        RuleFor(command => command.id).GreaterThan(0);
        RuleFor(command => command.kindId).InclusiveBetween(1L, 3L);
        RuleFor(command => command.joinedAt).NotEqual(default(DateTime));
    }
}
