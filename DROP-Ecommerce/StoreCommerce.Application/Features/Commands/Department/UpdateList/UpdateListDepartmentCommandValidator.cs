using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListDepartmentCommandValidator : AbstractValidator<UpdateListDepartmentCommand>
{
    public UpdateListDepartmentCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new UpdateDepartmentCommandValidator());
    }
}
