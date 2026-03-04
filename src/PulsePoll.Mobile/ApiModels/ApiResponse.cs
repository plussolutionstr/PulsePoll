namespace PulsePoll.Mobile.ApiModels;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ResponseMeta? Meta { get; init; }
}

public sealed class ResponseMeta
{
    public DateTime Timestamp { get; init; }
    public string? RequestId { get; init; }
}
