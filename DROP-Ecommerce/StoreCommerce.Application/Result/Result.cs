namespace StoreCommerce.Application.Result;

public sealed class Result<TContent> : IResult<Result<TContent>>
{
    private const string LegacyFailureCode = "General.Failure";
    private const string LegacyValidationCode = "Validation.Error";

    private Result(TContent content)
    {
        Content = content;
        IsSuccess = true;
        Errors = Array.Empty<Error>();
    }

    private Result(IEnumerable<Error> errors)
    {
        var errorList = errors?.ToArray()
            ?? throw new ArgumentNullException(nameof(errors));

        if (errorList.Length == 0)
            throw new ArgumentException("A failure result must contain at least one error.", nameof(errors));

        if (errorList.Any(error => error is null))
            throw new ArgumentException("A failure result cannot contain null errors.", nameof(errors));

        IsSuccess = false;
        Errors = Array.AsReadOnly(errorList);
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TContent? Content { get; }
    public IReadOnlyList<Error> Errors { get; }

    // Compatibilidade temporária com o contrato JSON anterior.
    public List<string> ListMessageErrors => Errors.Select(error => error.Message).ToList();

    public static Result<TContent> Success(TContent content) => new(content);

    public static Result<TContent> Failure(string message) =>
        Failure(Error.Failure(LegacyFailureCode, message));

    public static Result<TContent> Failure(string code, string message) =>
        Failure(Error.Failure(code, message));

    public static Result<TContent> Failure(Error error) =>
        new(new[] { error ?? throw new ArgumentNullException(nameof(error)) });

    public static Result<TContent> Failure(IEnumerable<Error> errors) => new(errors);

    public static Result<TContent> FailureFromList(List<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return Validation(errors.Select(message =>
            Error.Validation(LegacyValidationCode, message)));
    }

    public static Result<TContent> Validation(string code, string message) =>
        Failure(Error.Validation(code, message));

    public static Result<TContent> Validation(Error error) =>
        Failure(EnsureType(error, ErrorType.Validation));

    public static Result<TContent> Validation(IEnumerable<Error> errors) =>
        Failure(EnsureType(errors, ErrorType.Validation));

    public static Result<TContent> NotFound(string code, string message) =>
        Failure(Error.NotFound(code, message));

    public static Result<TContent> NotFound(Error error) =>
        Failure(EnsureType(error, ErrorType.NotFound));

    public static Result<TContent> NotFound(IEnumerable<Error> errors) =>
        Failure(EnsureType(errors, ErrorType.NotFound));

    public static Result<TContent> Conflict(string code, string message) =>
        Failure(Error.Conflict(code, message));

    public static Result<TContent> Conflict(Error error) =>
        Failure(EnsureType(error, ErrorType.Conflict));

    public static Result<TContent> Conflict(IEnumerable<Error> errors) =>
        Failure(EnsureType(errors, ErrorType.Conflict));

    public static Result<TContent> Unauthorized(string code, string message) =>
        Failure(Error.Unauthorized(code, message));

    public static Result<TContent> Unauthorized(Error error) =>
        Failure(EnsureType(error, ErrorType.Unauthorized));

    public static Result<TContent> Unauthorized(IEnumerable<Error> errors) =>
        Failure(EnsureType(errors, ErrorType.Unauthorized));

    public static Result<TContent> Forbidden(string code, string message) =>
        Failure(Error.Forbidden(code, message));

    public static Result<TContent> Forbidden(Error error) =>
        Failure(EnsureType(error, ErrorType.Forbidden));

    public static Result<TContent> Forbidden(IEnumerable<Error> errors) =>
        Failure(EnsureType(errors, ErrorType.Forbidden));

    public static Result<TContent> Unavailable(string code, string message) =>
        Failure(Error.Unavailable(code, message));

    public static Result<TContent> Unavailable(Error error) =>
        Failure(EnsureType(error, ErrorType.Unavailable));

    public static Result<TContent> Unavailable(IEnumerable<Error> errors) =>
        Failure(EnsureType(errors, ErrorType.Unavailable));

    private static Error EnsureType(Error error, ErrorType expectedType)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.Type != expectedType)
            throw new ArgumentException($"The error must be of type {expectedType}.", nameof(error));

        return error;
    }

    private static IEnumerable<Error> EnsureType(IEnumerable<Error> errors, ErrorType expectedType)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorList = errors.ToArray();
        if (errorList.Any(error => error is null || error.Type != expectedType))
            throw new ArgumentException($"All errors must be of type {expectedType}.", nameof(errors));

        return errorList;
    }
}
