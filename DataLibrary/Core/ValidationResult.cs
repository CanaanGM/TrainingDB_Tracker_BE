namespace DataLibrary.Core;

/// <summary>
/// Represents the outcome of a validation operation, extending the basic Result class to handle multiple validation errors.
/// </summary>
public class ValidationResult : Result
{
    /// <summary>
    /// Gets the list of validation error messages.
    /// </summary>
    public List<string> Errors { get; private set; } = new List<string>();

    /// <summary>
    /// Initializes a new instance of the ValidationResult class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the validation was successful.</param>
    /// <param name="errors">A list of error messages if the validation failed.</param>
    private ValidationResult(bool isSuccess, List<string> errors) 
        : base(isSuccess, errors.Count > 0 ? string.Join("; ", errors) : null, null)
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a success result for validation.
    /// </summary>
    /// <returns>A ValidationResult indicating success without any errors.</returns>
    public static ValidationResult Success()
    {
        return new ValidationResult(true, new List<string>());
    }

    /// <summary>
    /// Creates a failure result for validation.
    /// </summary>
    /// <param name="errors">The list of errors encountered during validation.</param>
    /// <returns>A ValidationResult indicating failure with a list of error messages.</returns>
    public static ValidationResult Failure(List<string> errors)
    {
        return new ValidationResult(false, errors);
    }

    /// <summary>
    /// Adds an error message to the validation result.
    /// </summary>
    /// <param name="error">The error message to add.</param>
    public void AddError(string error)
    {
        Errors.Add(error);
        ErrorMessage = string.Join("; ", Errors); // Updates the base error message for consistency, should i use ; or \n ??
    }

    /// <summary>
    /// Gets a value indicating whether the validation result is valid, which corresponds to being successful and having no errors.
    /// </summary>
    public bool IsValid => IsSuccess && !Errors.Any();
}

