namespace DataLibrary.Models;

public partial class TrainingSession
{
    public int Id { get; set; }

    public int? DurationInSeconds { get; set; }

    public int? TotalCaloriesBurned { get; set; }

    public string? Notes { get; set; }

    public int? Mood { get; set; }

    public string? Feeling { get; set; }

    public int? RateOfPerceivedExertionAvg { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; } = new List<TrainingSessionExerciseRecord>();

    public virtual User? User { get; set; }
}
