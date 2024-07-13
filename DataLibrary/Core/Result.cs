namespace DataLibrary.Core;

// TODO: tighten this and make it handle many senarios
// like a Successful operation but you still need to return a failure (IsSuccess=false) with the value of the operation.
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    // can extend this to take the error class itself 

    /// <summary>
    /// the Success state of the result
    /// </summary>
    /// <param name="value">The Value of the operation that you want to return</param>
    /// <returns>a success state with the value</returns>
    public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };

    /// <summary>
    /// The failure state of the result
    /// </summary>
    /// <param name="error">The error message you want to send up he chain</param>
    /// <param name="ex">The Exception that happened</param>
    /// <returns>a failure state with a message and optionally the Exception that was thrown</returns>
    public static Result<T> Failure(string error, Exception ex = null) => new Result<T> { IsSuccess = false, ErrorMessage = error, Exception = ex };
}
