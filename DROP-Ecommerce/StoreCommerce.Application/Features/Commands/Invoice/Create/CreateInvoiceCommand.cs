using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateInvoiceCommand(long orderId, long customerId, long enterpriseId, string number, string series, string accessKey, long typeId, long statusId, decimal totalAmount, decimal taxAmount, string fileUrl, DateTime? issuedAt) : IRequest<Result<Invoice>> { }
