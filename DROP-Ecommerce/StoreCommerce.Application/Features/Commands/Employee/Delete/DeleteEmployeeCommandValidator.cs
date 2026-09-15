using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(employee => employee.id).GreaterThan(0);
    }
}