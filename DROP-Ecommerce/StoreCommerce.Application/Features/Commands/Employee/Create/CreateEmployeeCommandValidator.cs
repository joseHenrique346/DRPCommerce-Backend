using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(employee => employee.enterpriseId).GreaterThan(0);
        RuleFor(employee => employee.enterpriseMembershipId).GreaterThan(0);
        RuleFor(employee => employee.fullName).NotEmpty().MaximumLength(255);
        RuleFor(employee => employee.positionId).GreaterThan(0);
        RuleFor(employee => employee.departmentId).GreaterThan(0);
        RuleFor(employee => employee.hiredAt).NotEqual(default(DateTime));
    }
}
