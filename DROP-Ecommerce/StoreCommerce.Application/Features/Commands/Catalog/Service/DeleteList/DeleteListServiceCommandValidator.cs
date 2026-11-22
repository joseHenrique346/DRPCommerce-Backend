using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListServiceCommandValidator : AbstractValidator<DeleteListServiceCommand>
{
    public DeleteListServiceCommandValidator()
    {
        RuleFor(service => service.ids).NotEmpty();
        RuleForEach(service => service.ids).GreaterThan(0);
    }
}
