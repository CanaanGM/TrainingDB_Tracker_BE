namespace DataLibrary.Core;
public class ExerciseQueryOptions
{
    // Filtering Options
    public string? TrainingTypeName { get; set; }
    public string? MuscleName { get; set; }
    public string? MuscleGroupName { get; set; }
    public int? MinimumDifficulty { get; set; }

    // Sorting and Pagination Options
    public SortBy SortBy { get; set; } = SortBy.NAME; // Default sorting by name
    public bool Ascending { get; set; } = true; // Sort direction
    public int PageNumber { get; set; } = 1; // Default to first page
    public int PageSize { get; set; } = 10; // Default page size



}

public enum SortBy
{
    NAME
    ,DIFFICULTY
    , MUSCLE_GROUP
    , TRAINING_TYPE

}