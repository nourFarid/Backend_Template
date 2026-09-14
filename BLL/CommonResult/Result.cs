namespace BLL.CommonResult;

public class Result
{
    protected readonly List<Error> _errors = [];
    public bool IsSuccess => !_errors.Any();
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors => _errors;

    protected Result() { }

    protected Result(Error error)
    {
        _errors.Add(error);
    }

    protected Result(IEnumerable<Error> errors)
    {
        _errors.AddRange(errors);
    }

    public static Result Success() => new();
    public static Result Fail(Error error) => new(error);
    public static Result Fail(IEnumerable<Error> errors) => new(errors);

    public static implicit operator Result(Error error) => Fail(error);
    public static implicit operator Result(List<Error> errors) => Fail(errors);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue? Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access the value of a failed result.");

    protected Result(TValue value)
    {
        _value = value;
    }

    protected Result(Error error) : base(error) { }
    protected Result(IEnumerable<Error> errors) : base(errors) { }

    public static Result<TValue> Success(TValue value) => new(value);
    public static new Result<TValue> Fail(Error error) => new(error);
    public static new Result<TValue> Fail(IEnumerable<Error> errors) => new(errors);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Fail(error);
    public static implicit operator Result<TValue>(List<Error> errors) => Fail(errors);
}
