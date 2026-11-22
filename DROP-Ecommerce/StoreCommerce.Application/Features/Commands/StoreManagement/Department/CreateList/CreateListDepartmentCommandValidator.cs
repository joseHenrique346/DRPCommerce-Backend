using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListDepartmentCommandValidator : AbstractValidator<CreateListDepartmentCommand>
{
    public CreateListDepartmentCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new CreateDepartmentCommandValidator());
    }
}
