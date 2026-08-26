namespace DropCommerce.Application.Result;

public sealed record Error
{
    public Error(string code, string message, ErrorType type)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("The error code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("The error message is required.", nameof(message));

        Code = code;
        Message = message;
        Type = type;
    }

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    public static Error Unavailable(string code, string message) =>
        new(code, message, ErrorType.Unavailable);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);
}
