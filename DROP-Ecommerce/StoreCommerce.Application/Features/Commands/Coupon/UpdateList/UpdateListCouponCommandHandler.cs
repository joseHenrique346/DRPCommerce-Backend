using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListCouponCommandHandler(ICouponRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateListCouponCommand, Result<List<Coupon>>>
{
    public async Task<Result<List<Coupon>>> Handle(UpdateListCouponCommand request, CancellationToken cancellationToken)
    {
        var listId = request.commands.Select(command => command.id).ToList();

        if (listId.Count != listId.Distinct().Count())
            return Result<List<Coupon>>.Validation($"{nameof(UpdateListCouponCommand)}.DuplicateIds", "A lista contém ids duplicados.");

        var coupons = (await repository.GetListByListIdAsync(listId, cancellationToken)).ToDictionary(coupon => coupon.Id);

        var missingId = listId.FirstOrDefault(id => !coupons.ContainsKey(id));
        if (missingId > 0)
            return Result<List<Coupon>>.NotFound("Coupon.NotFound", $"Coupon com id {missingId} não encontrado.");

        var error = await CouponRules.ValidateUniqueCodesAsync(repository, request.commands.Select(command => (command.id, command.code)).ToList(), cancellationToken);

        if (error is not null)
            return Result<List<Coupon>>.Failure(error);

        var result = new List<Coupon>(request.commands.Count);

        foreach (var command in request.commands)
        {
            var coupon = coupons[command.id];
            var conditions = CouponRules.BuildConditions(command.conditions);

            if (coupon.WouldChangeLockedTerms(command.code, command.typeId, command.discountValue, command.maxDiscountCap, command.isSingleUse, command.startsAt, conditions))
                return Result<List<Coupon>>.Validation("Coupon.InUse",
                    $"O cupom {coupon.Code} já foi utilizado: apenas expiresAt, maxUses e isActive podem ser alterados.");

            if (command.maxUses.HasValue && command.maxUses.Value < coupon.UsedCount)
                return Result<List<Coupon>>.Validation("Coupon.MaxUsesBelowUsage",
                    $"O cupom {coupon.Code} já tem {coupon.UsedCount} usos; maxUses não pode ser menor.");

            coupon.UpdateDetails(command.code, command.typeId, command.discountValue, command.maxDiscountCap, command.maxUses,
                command.isSingleUse, command.startsAt, command.expiresAt, conditions);
            coupon.SetActive(command.isActive);
            result.Add(coupon);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<List<Coupon>>.Success(result);
    }
}
