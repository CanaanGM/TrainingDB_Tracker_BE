namespace DataLibrary.Dtos;
public class TrainingSessionReadDto
{
    public int Id { get; set; }
    public int? DurationInSeconds { get; set; }
    public int? Calories { get; set; }
    public string? Notes { get; set; }
    public int? Mood { get; set; }
    public DateTime? CreatedAt { get; set; }
    public virtual ICollection<ExerciseRecordReadDto> ExerciseRecords { get; set; } = new List<ExerciseRecordReadDto>();
    public virtual ICollection<TrainingTypeReadDto> TrainingTypes { get; set; } = new List<TrainingTypeReadDto>();
}

public class TrainingSessionWriteDto
{
    public int? DurationInSeconds { get; set; }
    public int? Calories { get; set; }
    public string? Notes { get; set; }
    public int? Mood { get; set; }
    public List<ExerciseRecordWriteDto> ExerciseRecords { get; set; }
    public string? CreatedAt { get; set; }

}

public class ExerciseRecordReadDto
{
    public int Id { get; set; }
    public int? Repetitions { get; set; }
    public int? TimerInSeconds { get; set; }
    public int? DistanceInMeters { get; set; }
    public double? WeightUsedKg { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string ExerciseName { get; set; }
    public string MuscleGroup { get; set; }
}
public class ExerciseRecordWriteDto
{
    public int? Repetitions { get; set; }
    public int? TimerInSeconds { get; set; }
    public int? DistanceInMeters { get; set; }
    public double? WeightUsedKg { get; set; }
    public string? Notes { get; set; }
    public string ExerciseName { get; set; }
}