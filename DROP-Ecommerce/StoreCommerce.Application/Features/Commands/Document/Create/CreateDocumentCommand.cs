using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateDocumentCommand(long enterpriseId, long referenceId, string referenceType, long typeId, string number, string fileUrl, long statusId, DateTime issuedAt, DateTime? expiresAt) : IRequest<Result<Document>> { }
