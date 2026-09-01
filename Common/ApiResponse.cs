namespace PosApi.Common;

/// <summary>
/// Standard envelope returned by every API endpoint for consistent client handling.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Data = default
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse<object> SuccessResponse(string message = "Request successful")
    {
        return new ApiResponse<object>
        {
            Success = true,
            Message = message
        };
    }
}
