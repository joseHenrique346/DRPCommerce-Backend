using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropTransactionCommandValidator : AbstractValidator<CreateDropTransactionCommand>
{
    public CreateDropTransactionCommandValidator()
    {
        RuleFor(x => x.dropOrderId).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.typeId).GreaterThan(0);
        RuleFor(x => x.methodId).GreaterThan(0);
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.amount).GreaterThan(0);
        RuleFor(x => x.fee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.gatewayReference).NotEmpty();
        RuleFor(x => x.gatewayProvider).NotEmpty();
        RuleFor(x => x.gatewayPayload).NotEmpty();
    }
}