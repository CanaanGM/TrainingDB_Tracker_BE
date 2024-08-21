using DataLibrary.Core;
using DataLibrary.Models;

namespace DataLibrary.Helpers;

//TODO: was an attempt at refactoring, but alas, it needed more work than actually worth it.
public static class ExerciseHelpers
{
    public static async Task<Result<Dictionary<string, Exercise>>> GetRelatedExercisesDictionaryAsync(
        Func<List<string>, CancellationToken, Task<Dictionary<string, Exercise>>> fetchExercisesFunc,
        List<string> exerciseNames,
        CancellationToken cancellationToken)
    {
        var result = await fetchExercisesFunc(exerciseNames, cancellationToken);

        if (result.Count == exerciseNames.Count)
            return Result<Dictionary<string, Exercise>>.Success(result);

        var missingExercises = exerciseNames
            .Where(x => !result.Keys.Contains(x))
            .Distinct();
        var errorMessage = string.Join("\n ", missingExercises);
        return Result<Dictionary<string, Exercise>>.Failure(
            $"Some exercises are not in the database:\n {errorMessage}");
    }
    
}