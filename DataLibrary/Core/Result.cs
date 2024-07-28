namespace DataLibrary.Core;

// TODO: tighten this and make it handle many senarios
// like a Successful operation but you still need to return a failure (IsSuccess=false) with the value of the operation.
public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    protected Result(bool isSuccess, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result Success() => new Result(true, null, null);

    public static Result Failure(string error, Exception? ex = null) => new Result(false, error, ex);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? errorMessage, Exception? exception)
        : base(isSuccess, errorMessage, exception)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null, null);

    public static Result<T> Failure(string error, Exception? ex = null) => new Result<T>(false, default, error, ex);
}
