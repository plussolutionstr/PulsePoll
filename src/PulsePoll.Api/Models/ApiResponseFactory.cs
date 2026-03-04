namespace PulsePoll.Api.Models;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T data, string requestId) => new()
    {
        Success = true,
        Data = data,
        Meta = new ResponseMeta { RequestId = requestId }
    };

    public static ApiResponse Success(string requestId) => new()
    {
        Success = true,
        Meta = new ResponseMeta { RequestId = requestId }
    };

    public static ApiErrorResponse Error(string code, string message, string requestId, List<FieldError>? details = null) => new()
    {
        Success = false,
        Error = new ErrorDetail { Code = code, Message = message, Details = details },
        Meta = new ResponseMeta { RequestId = requestId }
    };

    public static ApiErrorResponse ValidationError(Dictionary<string, string[]> errors, string requestId) => new()
    {
        Success = false,
        Error = new ErrorDetail
        {
            Code = ErrorCodes.ValidationError,
            Message = "Doğrulama hatası oluştu.",
            Details = errors.SelectMany(e =>
                e.Value.Select(msg => new FieldError { Field = e.Key, Message = msg })).ToList()
        },
        Meta = new ResponseMeta { RequestId = requestId }
    };

    public static ApiErrorResponse NotFound(string resource, string requestId) => new()
    {
        Success = false,
        Error = new ErrorDetail { Code = ErrorCodes.NotFound, Message = $"{resource} bulunamadı." },
        Meta = new ResponseMeta { RequestId = requestId }
    };

    public static ApiErrorResponse Unauthorized(string requestId, string message = "Kimlik doğrulama gerekli.") => new()
    {
        Success = false,
        Error = new ErrorDetail { Code = ErrorCodes.Unauthorized, Message = message },
        Meta = new ResponseMeta { RequestId = requestId }
    };

    public static ApiErrorResponse Forbidden(string requestId) => new()
    {
        Success = false,
        Error = new ErrorDetail { Code = ErrorCodes.Forbidden, Message = "Bu işlem için yetkiniz yok." },
        Meta = new ResponseMeta { RequestId = requestId }
    };

    public static PagedApiResponse<T> Paged<T>(List<T> items, PaginationMeta pagination, string requestId) => new()
    {
        Success    = true,
        Data       = items,
        Pagination = pagination,
        Meta       = new ResponseMeta { RequestId = requestId }
    };
}
