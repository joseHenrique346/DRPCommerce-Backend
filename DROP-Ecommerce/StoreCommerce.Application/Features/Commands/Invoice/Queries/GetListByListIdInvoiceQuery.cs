using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetListByListIdInvoiceQuery(List<long> listId) : IRequest<Result<List<Invoice>>> { }
