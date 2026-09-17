using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEnterpriseCommandValidator : AbstractValidator<DeleteListEnterpriseCommand>
{
    public DeleteListEnterpriseCommandValidator()
    {
        RuleFor(enterprise => enterprise.ids).NotEmpty();
        RuleForEach(enterprise => enterprise.ids).GreaterThan(0);
    }
}
