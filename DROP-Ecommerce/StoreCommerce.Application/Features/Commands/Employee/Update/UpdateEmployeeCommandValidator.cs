using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(employee => employee.id).GreaterThan(0);
        RuleFor(employee => employee.enterpriseId).GreaterThan(0);
        RuleFor(employee => employee.fullName).NotEmpty().MaximumLength(200);
        RuleFor(employee => employee.email).NotNull();
        RuleFor(employee => employee.email.Value).NotEmpty().MaximumLength(255).EmailAddress();
        RuleFor(employee => employee.passwordHash).NotEmpty().MaximumLength(500);
        RuleFor(employee => employee.roleId).NotNull();
        RuleFor(employee => employee.roleId.Id).GreaterThan(0);
        RuleFor(employee => employee.departmentId).NotNull();
        RuleFor(employee => employee.departmentId.Id).GreaterThan(0);
        RuleFor(employee => employee.hiredAt).NotEqual(default(DateTime));
    }
}