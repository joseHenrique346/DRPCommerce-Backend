using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropOrderItemCommandValidator : AbstractValidator<UpdateListDropOrderItemCommand>
{
    public UpdateListDropOrderItemCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropOrderItemCommandValidator());
    }
}
