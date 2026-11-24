using FluentValidation;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(Request => Request.CurrentPassword).NotEmpty().MaximumLength(128);
    }
}
