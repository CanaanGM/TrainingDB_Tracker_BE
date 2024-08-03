namespace DataLibrary.Core;

// TODO: Use fluent API style
// TODO: maybe use logging here too
// TODO: use status codes

public class Result
{
    public bool IsSuccess { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    protected Result(bool isSuccess, string? successMessage, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result Success(string? successMessage = "Operation Succeded") => new Result(
        true,
        successMessage,
        null,
        null
    );

    public static Result Failure(string error, Exception? ex = null) => new Result(
        false,
        null,
        error,
        ex
    );
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(
        bool isSuccess,
        T? value,
        string? errorMessage,
        Exception? exception,
        string? successMessage = "Operation succeeded")
        : base(isSuccess, successMessage, errorMessage, exception)
    {
        Value = value;
    }

    public static Result<T> Success(T value, string successMessage = "operation succeeded") => new Result<T>(
        isSuccess: true,
        value: value,
        errorMessage: null,
        exception: null,
        successMessage: successMessage
    );

    public static Result<T> Failure(string error, Exception? ex = null) => new Result<T>(
        isSuccess: false,
        value: default,
        errorMessage: error,
        exception: ex,
        successMessage: default
    );
}