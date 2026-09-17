using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEmployeeCommandValidator : AbstractValidator<CreateListEmployeeCommand>
{
    public CreateListEmployeeCommandValidator()
    {
        RuleFor(employee => employee.commands).NotEmpty();
        RuleForEach(employee => employee.commands).SetValidator(new CreateEmployeeCommandValidator());
    }
}
