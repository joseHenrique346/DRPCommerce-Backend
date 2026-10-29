using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(Request => Request.QuantityChange).NotEqual(0);
        RuleFor(Request => Request.Reason).NotEmpty().MaximumLength(500);
        RuleFor(Request => Request.IdempotencyKey).NotEmpty().MaximumLength(100);
        RuleFor(Request => Request.ExpectedVersion).GreaterThan(0).When(Request => Request.ExpectedVersion.HasValue);
    }
}

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(Command => Command.ProductVariantId).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull().SetValidator(new AdjustStockRequestValidator());
    }
}

public sealed class ReserveStockRequestValidator : AbstractValidator<ReserveStockRequest>
{
    public ReserveStockRequestValidator()
    {
        RuleFor(Request => Request.IdempotencyKey).NotEmpty().MaximumLength(100);
        RuleFor(Request => Request.Reference).MaximumLength(200);
        RuleFor(Request => Request.Items).Cascade(CascadeMode.Stop).NotEmpty().Must(Items => Items.Count <= 100)
            .WithMessage("A reserva permite no máximo 100 SKUs.");
        RuleFor(Request => Request.Items).Must(Items => Items is null || Items.Where(Item => Item is not null)
            .Select(Item => Item.ProductVariantId).Distinct().Count() == Items.Count)
            .WithMessage("A reserva contém SKUs duplicados.");
        RuleForEach(Request => Request.Items).NotNull().ChildRules(Line =>
        {
            Line.RuleFor(Item => Item.ProductVariantId).GreaterThan(0);
            Line.RuleFor(Item => Item.Quantity).InclusiveBetween(1, 10000);
        });
    }
}

public sealed class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(Command => Command.Request).NotNull().SetValidator(new ReserveStockRequestValidator());
    }
}

public sealed class ReleaseStockCommandValidator : AbstractValidator<ReleaseStockCommand>
{
    public ReleaseStockCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
    }
}

public sealed class CommitStockCommandValidator : AbstractValidator<CommitStockCommand>
{
    public CommitStockCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
    }
}

public sealed class GetStockReservationQueryValidator : AbstractValidator<GetStockReservationQuery>
{
    public GetStockReservationQueryValidator()
    {
        RuleFor(Query => Query.Id).GreaterThan(0);
    }
}

public sealed class GetVariantStockQueryValidator : AbstractValidator<GetVariantStockQuery>
{
    public GetVariantStockQueryValidator()
    {
        RuleFor(Query => Query.Id).GreaterThan(0);
    }
}

public sealed class GetStockMovementsQueryValidator : AbstractValidator<GetStockMovementsQuery>
{
    public GetStockMovementsQueryValidator()
    {
        RuleFor(Query => Query.ProductVariantId).GreaterThan(0).When(Query => Query.ProductVariantId.HasValue);
        RuleFor(Query => Query.Page).InclusiveBetween(1, 100000);
        RuleFor(Query => Query.PageSize).InclusiveBetween(1, 100);
    }
}
