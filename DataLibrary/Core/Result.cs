namespace DataLibrary.Core;

// TODO: Use fluent API style
// TODO: maybe use logging here too
// TODO: use status codes

/// <summary>
/// Represents the outcome of an operation without a return value, indicating success or failure along with optional error information.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets an optional error message describing why the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets an optional exception associated with the failure of the operation.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Initializes a new instance of the Result class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errorMessage">The error message describing the reason for failure, if any.</param>
    /// <param name="exception">The exception associated with the failure, if any.</param>
    protected Result(bool isSuccess, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    /// <returns>A successful result object.</returns>
    public static Result Success() => new Result(true, null, null);

    /// <summary>
    /// Creates a failure result with an error message and optionally an exception.
    /// </summary>
    /// <param name="error">The error message describing the reason for failure.</param>
    /// <param name="ex">Optional exception associated with the failure.</param>
    /// <returns>A failure result object.</returns>
    public static Result Failure(string error, Exception? ex = null) => new Result(false, error, ex);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type T, indicating success or failure along with the return value.
/// </summary>
/// <typeparam name="T">The type of the return value associated with the operation.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value produced by the operation if it was successful.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the Result<T> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="value">The value returned by the operation, if any.</param>
    /// <param name="errorMessage">The error message describing the reason for failure, if any.</param>
    /// <param name="exception">The exception associated with the failure, if any.</param>
    private Result(bool isSuccess, T? value, string? errorMessage, Exception? exception)
        : base(isSuccess, errorMessage, exception)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a success result with the specified value.
    /// </summary>
    /// <param name="value">The value to return as part of the success result.</param>
    /// <returns>A successful result object containing the specified value.</returns>
    public static Result<T> Success(T value) => new Result<T>(true, value, null, null);

    /// <summary>
    /// Creates a failure result with an error message and optionally an exception.
    /// </summary>
    /// <param name="error">The error message describing the reason for failure.</param>
    /// <param name="ex">Optional exception associated with the failure.</param>
    /// <returns>A failure result object without a value.</returns>
    public static Result<T> Failure(string error, Exception? ex = null) => new Result<T>(false, default, error, ex);
}