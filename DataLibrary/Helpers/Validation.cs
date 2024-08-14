using DataLibrary.Core;

namespace DataLibrary.Helpers;

public static class Validation
{
    
    /// <summary>
    /// takes in an object and goes thru it's sring props and validates that they're neither null or empty
    /// </summary>
    /// <param name="dto"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>a result of Success or Failure</returns>
    public static Result ValidateDtoStrings<T>(T dto)
    {
        List<string> errors = [];

        foreach (var property in typeof(T).GetProperties())
        {
            if (property.PropertyType == typeof(string))
            {
                if(property.Name == "Notes") continue; // don't validate notes yo!
                var propertyValue = (string)property.GetValue(dto);
                
                if (propertyValue is null)
                {
                    errors.Add($"{property.Name} cannot be null");
                    continue;
                }
                if(string.IsNullOrEmpty(propertyValue))
                    errors.Add($"{property.Name} cannot be empty");
                    
            }
        }

        return errors.Count > 0
            ? Result.Failure(string.Join(", ", errors))
            : Result.Success($"{dto!.GetType().Name} validation succeeded");
    }
    
    /// <summary>
    /// Validates that all ICollection properties in the given DTO are not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the DTO being validated.</typeparam>
    /// <param name="dto">The DTO instance to validate.</param>
    /// <returns>A Result indicating success or failure of the validation, with appropriate error messages.</returns>
    public static Result ValidateDtoICollections<T>(T dto)
    {
        var errors = new List<string>();

        foreach (var property in typeof(T).GetProperties())
        {
            if (property.PropertyType.IsGenericType && 
                typeof(ICollection<>).IsAssignableFrom(property.PropertyType.GetGenericTypeDefinition()))
            {
                var collection = property.GetValue(dto) as System.Collections.ICollection;
                if (collection == null || collection.Count == 0)
                {
                    errors.Add($"{property.Name} cannot be null or empty");
                }
            }
        }

        return errors.Count > 0
            ? Result.Failure(string.Join(", ", errors))
            : Result.Success($"{dto!.GetType().Name} validation succeeded");
    }
}