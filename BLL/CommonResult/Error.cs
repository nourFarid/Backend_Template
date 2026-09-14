namespace BLL.CommonResult;

public class Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }
    public string MessageKey { get; }

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
        MessageKey = code;
    }

    public Error(string code, string description, ErrorType type, string messageKey)
    {
        Code = code;
        Description = description;
        Type = type;
        MessageKey = messageKey;
    }

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);
    public static Error InvalidCredentials(string code, string description) => new(code, description, ErrorType.InvalidCredentials);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error Locked(string code, string description) => new(code, description, ErrorType.Locked);
}
