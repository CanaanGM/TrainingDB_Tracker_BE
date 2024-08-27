using SharedLibrary.Core;
using SharedLibrary.Dtos;

namespace SharedLibrary.Helpers;

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
    
     /// <summary>
    /// Validates that the order numbers for TrainingWeeks, TrainingDays, and Blocks in the provided TrainingPlanWriteDto
    /// are sequential and start from 1.
    /// </summary>
    /// <param name="planDto">The TrainingPlanWriteDto object containing the training plan data to validate.</param>
    /// <param name="validationError">An output parameter that contains the validation error message if validation fails.</param>
    /// <returns>True if all order numbers are sequential and valid; otherwise, false.</returns>
    public static bool ValidateOrderNumbers(TrainingPlanWriteDto planDto, out string validationError)
    {
        validationError = string.Empty;

        // Validate TrainingWeeks OrderNumbers
        if (!IsSequential(planDto.TrainingWeeks.Select(w => w.OrderNumber)))
        {
            validationError = "Training weeks must have sequential order numbers starting from 1.";
            return false;
        }

        // Validate TrainingDays OrderNumbers within each week
        foreach (var week in planDto.TrainingWeeks)
        {
            if (!IsSequential(week.TrainingDays.Select(d => d.OrderNumber)))
            {
                validationError = $"Training days in '{week.Name}' must have sequential order numbers starting from 1.";
                return false;
            }

            // Validate Blocks OrderNumbers within each day
            foreach (var day in week.TrainingDays)
            {
                if (!IsSequential(day.Blocks.Select(b => b.OrderNumber)))
                {
                    validationError = $"Blocks in '{day.Name}' must have sequential order numbers starting from 1.";
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if a given collection of integers is sequential, starting from 1.
    /// </summary>
    /// <param name="numbers">The collection of integers representing order numbers.</param>
    /// <returns>True if the numbers are sequential starting from 1; otherwise, false.</returns>
    public static bool IsSequential(IEnumerable<int> numbers)
    {
        var orderedNumbers = numbers.OrderBy(n => n).ToList();
        return !orderedNumbers.Where((t, i) => t != i + 1).Any();
    }


    /// <summary>
    /// Validated if there's any exercises in a plan, as it should have at least one
    /// </summary>
    /// <param name="newPlanDto">the creation dto</param>
    /// <param name="validationError">if there were no exercises found</param>
    /// <returns></returns>
    public static bool ValidateTrainingPlan(TrainingPlanWriteDto newPlanDto, out string validationError)
    {
        validationError = string.Empty;
        if (newPlanDto.TrainingWeeks.Count == 0 || !newPlanDto.TrainingWeeks
                .Any(week => week.TrainingDays.Any(day => day.Blocks.Any(block => block.BlockExercises.Any()))))
        {
            validationError = "The training plan must have at least one week with one day and one exercise.";
            return false;
        }

        return true;
    }
    
}