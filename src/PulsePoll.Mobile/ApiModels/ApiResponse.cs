namespace PulsePoll.Mobile.ApiModels;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiErrorDetail? Error { get; init; }
    public ResponseMeta? Meta { get; init; }
}

public sealed class PagedApiResponse<T>
{
    public bool Success { get; init; }
    public List<T>? Data { get; init; }
    public ApiErrorDetail? Error { get; init; }
    public PaginationMeta? Pagination { get; init; }
    public ResponseMeta? Meta { get; init; }
}

public sealed class ApiErrorDetail
{
    public string? Code { get; init; }
    public string? Message { get; init; }
    public List<ApiFieldError>? Details { get; init; }
}

public sealed class ApiFieldError
{
    public string? Field { get; init; }
    public string? Message { get; init; }
}

public sealed class ResponseMeta
{
    public DateTime Timestamp { get; init; }
    public string? RequestId { get; init; }
}

public sealed class PaginationMeta
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
