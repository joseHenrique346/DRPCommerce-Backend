using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(employee => employee.id).GreaterThan(0);
        RuleFor(employee => employee.fullName).NotEmpty().MaximumLength(255);
        RuleFor(employee => employee.positionId).GreaterThan(0);
        RuleFor(employee => employee.departmentId).GreaterThan(0);
        RuleFor(employee => employee.hiredAt).NotEqual(default(DateTime));
    }
}
