using FluentValidation;
using MediatR;
using StoreCommerce.Application.Result;
using System.Text.RegularExpressions;

namespace StoreCommerce.Application.Features;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = failures.Select(failure => Error.Validation(
            CreateErrorCode(failure.PropertyName),
            failure.ErrorMessage));

        return TResponse.Validation(errors);
    }

    private static string CreateErrorCode(string? propertyName)
    {
        var stablePropertyName = string.IsNullOrWhiteSpace(propertyName)
            ? "Request"
            : Regex.Replace(propertyName, @"\[\d+\]", "[]");

        return $"{typeof(TRequest).Name}.{stablePropertyName}";
    }
}
