using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEnterpriseCommandValidator : AbstractValidator<CreateListEnterpriseCommand>
{
    public CreateListEnterpriseCommandValidator()
    {
        RuleFor(enterprise => enterprise.commands).NotEmpty();
        RuleForEach(enterprise => enterprise.commands).SetValidator(new CreateEnterpriseCommandValidator());
    }
}
