namespace DataLibrary.Models;

public partial class ExerciseRecord
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? UserExerciseId { get; set; }

    public int? Repetitions { get; set; }

    public int? TimerInSeconds { get; set; }

    public int? DistanceInMeters { get; set; }

    public double? WeightUsedKg { get; set; }

    public string? Notes { get; set; }

    public int? RestInSeconds { get; set; }

    public int? Incline { get; set; }

    public int? Speed { get; set; }

    public int? HeartRateAvg { get; set; }

    public int? KcalBurned { get; set; }

    public int? Mood { get; set; }

    public double? RateOfPerceivedExertion { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; } = new List<TrainingSessionExerciseRecord>();

    public virtual User? User { get; set; }

    public virtual UserExercise? UserExercise { get; set; }
}
