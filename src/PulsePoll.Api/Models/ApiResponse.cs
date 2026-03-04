namespace PulsePoll.Api.Models;

public class ApiResponse
{
    public bool Success { get; init; }
    public ResponseMeta Meta { get; init; } = new();
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }
}

public class ApiErrorResponse : ApiResponse
{
    public ErrorDetail Error { get; init; } = null!;
}

public class ResponseMeta
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string RequestId { get; init; } = string.Empty;
}

public class ErrorDetail
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public List<FieldError>? Details { get; init; }
    public string? StackTrace { get; init; }
}

public class FieldError
{
    public string Field { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public class PaginationMeta
{
    public int Page       { get; init; }
    public int PageSize   { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public class PagedApiResponse<T> : ApiResponse
{
    public List<T>       Data       { get; init; } = [];
    public PaginationMeta Pagination { get; init; } = null!;
}
