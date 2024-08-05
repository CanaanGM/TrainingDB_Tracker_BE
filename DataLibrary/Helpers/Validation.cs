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
}