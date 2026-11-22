using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListServiceCommandValidator : AbstractValidator<UpdateListServiceCommand>
{
    public UpdateListServiceCommandValidator()
    {
        RuleFor(service => service.commands).NotEmpty();
        RuleForEach(service => service.commands).SetValidator(new UpdateServiceCommandValidator());
    }
}
