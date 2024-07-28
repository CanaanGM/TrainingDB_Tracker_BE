namespace DataLibrary.Models;

public partial class BlockExercise
{
    public int Id { get; set; }

    public int? BlockId { get; set; }

    public int? ExerciseId { get; set; }

    public int? OrderNumber { get; set; }

    public string? Instructions { get; set; }

    public int? Repetitions { get; set; }

    public int? TimerInSeconds { get; set; }

    public int? DistanceInMeters { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Block? Block { get; set; }

    public virtual Exercise? Exercise { get; set; }
}
