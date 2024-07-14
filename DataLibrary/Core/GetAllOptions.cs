namespace DataLibrary.Core;
public class ExerciseQueryOptions
{
    /// <summary>
    /// The name of the muscle group to filter exercises by.
    /// </summary>
    public string? MuscleGroupName { get; set; }

    /// <summary>
    /// The name of the muscle to filter exercises by.
    /// </summary>
    public string? MuscleName { get; set; }

    /// <summary>
    /// The name of the training type to filter exercises by.
    /// </summary>
    public string? TrainingTypeName { get; set; }

    /// <summary>
    /// The minimum difficulty level to filter exercises by.
    /// </summary>
    public int? MinimumDifficulty { get; set; }

    /// <summary>
    /// The maximum difficulty level to filter exercises by.
    /// </summary>
    public int? MaximumDifficulty { get; set; }

    /// <summary>
    /// The page number for pagination.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// The page size for pagination.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// The field to sort the results by.
    /// </summary>
    public SortBy SortBy { get; set; } = SortBy.NAME;

    /// <summary>
    /// Whether to sort the results in ascending order.
    /// </summary>
    public bool Ascending { get; set; } = true;
}


public enum SortBy
{
    NAME
    , DIFFICULTY
    , MUSCLE_GROUP
    , TRAINING_TYPE

}