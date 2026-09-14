namespace Shared.DTO;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; } = true;
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ApiErrorItem> Errors { get; set; } = [];
    public string TraceId { get; set; } = string.Empty;

    public static ApiResponse<T> Ok(T? data = default, string? message = null, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Message = message ?? "Success",
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message, List<ApiErrorItem>? errors = null, int statusCode = 400, string traceId = "")
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? [],
            TraceId = traceId
        };
    }

    public static ApiResponse<T> SuccessResponse(T? data, string message = "", int statusCode = 200) => Ok(data, message, statusCode);
    public static ApiResponse<T> ErrorResponse(string message, List<ApiErrorItem>? errors = null, int statusCode = 400, string traceId = "") => Fail(message, errors, statusCode, traceId);
}

public class ApiErrorItem
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public ApiErrorItem() { }

    public ApiErrorItem(string code, string description, string type)
    {
        Code = code;
        Description = description;
        Type = type;
    }
}

public class PaginatedResult<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public IReadOnlyList<T> Items { get; set; } = [];

    public PaginatedResult() { }

    public PaginatedResult(int pageIndex, int pageSize, int totalCount, IReadOnlyList<T> items)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
        Items = items;
    }

    public PaginatedResult(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}
