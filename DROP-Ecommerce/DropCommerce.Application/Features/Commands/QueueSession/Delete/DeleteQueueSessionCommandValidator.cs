using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteQueueSessionCommandValidator : AbstractValidator<DeleteQueueSessionCommand>
{
    public DeleteQueueSessionCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}