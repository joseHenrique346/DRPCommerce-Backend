using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListServiceCommandValidator : AbstractValidator<CreateListServiceCommand>
{
    public CreateListServiceCommandValidator()
    {
        RuleFor(service => service.commands).NotEmpty();
        RuleForEach(service => service.commands).SetValidator(new CreateServiceCommandValidator());
    }
}
