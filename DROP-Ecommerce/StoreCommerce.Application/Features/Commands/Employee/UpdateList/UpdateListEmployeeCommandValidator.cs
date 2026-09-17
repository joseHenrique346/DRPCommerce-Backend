using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEmployeeCommandValidator : AbstractValidator<UpdateListEmployeeCommand>
{
    public UpdateListEmployeeCommandValidator()
    {
        RuleFor(employee => employee.commands).NotEmpty();
        RuleForEach(employee => employee.commands).SetValidator(new UpdateEmployeeCommandValidator());
    }
}
