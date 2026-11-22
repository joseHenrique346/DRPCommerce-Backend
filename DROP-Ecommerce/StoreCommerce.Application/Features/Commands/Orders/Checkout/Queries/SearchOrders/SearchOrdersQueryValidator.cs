using FluentValidation;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchOrdersQueryValidator : AbstractValidator<SearchOrdersQuery>
{
    private static readonly string[] Sorts = ["newest", "oldest", "total_desc", "total_asc"];

    public SearchOrdersQueryValidator()
    {
        RuleFor(query => query.Number).GreaterThan(0).When(query => query.Number.HasValue);
        RuleForEach(query => query.StatusIds)
            .Must(statusId => BaseStaticEntity.GetAll<OrderStatus>().Any(status => status.Id == statusId)).WithMessage("{PropertyName} inválido.");
        RuleForEach(query => query.PaymentStatusIds)
            .Must(statusId => BaseStaticEntity.GetAll<OrderPaymentStatus>().Any(status => status.Id == statusId)).WithMessage("{PropertyName} inválido.");
        RuleFor(query => query.StatusIds).Must(ids => ids is null || ids.Count <= 10).WithMessage("Informe no máximo 10 status.");
        RuleFor(query => query.PaymentStatusIds).Must(ids => ids is null || ids.Count <= 10).WithMessage("Informe no máximo 10 status de pagamento.");
        RuleFor(query => query.CustomerId).GreaterThan(0).When(query => query.CustomerId.HasValue);
        RuleFor(query => query.CouponId).GreaterThan(0).When(query => query.CouponId.HasValue);
        RuleFor(query => query.CreatedTo).GreaterThanOrEqualTo(query => query.CreatedFrom)
            .When(query => query.CreatedFrom.HasValue && query.CreatedTo.HasValue).WithMessage("createdTo deve ser posterior a createdFrom.");
        RuleFor(query => query.MinTotal).GreaterThanOrEqualTo(0).When(query => query.MinTotal.HasValue);
        RuleFor(query => query.MaxTotal).GreaterThanOrEqualTo(query => query.MinTotal ?? 0).When(query => query.MaxTotal.HasValue)
            .WithMessage("maxTotal deve ser maior ou igual a minTotal.");
        RuleFor(query => query.Sort).Must(sort => sort is null || Sorts.Contains(sort))
            .WithMessage("sort deve ser newest, oldest, total_desc ou total_asc.");
        RuleFor(query => query.Page).InclusiveBetween(1, 100000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
