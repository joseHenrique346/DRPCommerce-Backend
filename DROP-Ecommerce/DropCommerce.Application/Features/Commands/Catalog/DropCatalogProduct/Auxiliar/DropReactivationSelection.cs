using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public sealed record DropReactivationSelection(int Available, List<DropProduct> Products);
