namespace PulsePoll.Api.Models;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InvalidFormat = "INVALID_FORMAT";

    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string AccountNotApproved = "ACCOUNT_NOT_APPROVED";

    public const string NotFound = "NOT_FOUND";
    public const string AlreadyExists = "ALREADY_EXISTS";

    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";

    public const string InternalError = "INTERNAL_ERROR";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
}
