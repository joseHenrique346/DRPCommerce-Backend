using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEmployeeCommandValidator : AbstractValidator<DeleteListEmployeeCommand>
{
    public DeleteListEmployeeCommandValidator()
    {
        RuleFor(employee => employee.ids).NotEmpty();
        RuleForEach(employee => employee.ids).GreaterThan(0);
    }
}
