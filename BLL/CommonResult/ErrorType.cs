namespace BLL.CommonResult;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Forbidden = 4,
    InvalidCredentials = 5,
    Conflict = 6,
    Locked = 7
}
