namespace DataLibrary.Core;
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? ErrorMessage { get; set; }

    public Exception? Exception { get; set; }

    // can extend this to take the error class itself 

    public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };

    public static Result<T> Failure(string error, Exception ex = null) => new Result<T> { IsSuccess = false, ErrorMessage = error, Exception = ex };
}
