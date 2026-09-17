using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEnterpriseCommandValidator : AbstractValidator<UpdateListEnterpriseCommand>
{
    public UpdateListEnterpriseCommandValidator()
    {
        RuleFor(enterprise => enterprise.commands).NotEmpty();
        RuleForEach(enterprise => enterprise.commands).SetValidator(new UpdateEnterpriseCommandValidator());
    }
}
