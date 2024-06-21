namespace DataLibrary;
public class TrainingSessionExerciseRecord
{
    public int Id { get; set; }
    public TrainingSession TrainingSession { get; set; }
    public int TrainingSessionId { get; set; }
    public ExerciseRecord ExerciseRecord { get; set; }
    public int ExerciseRecordId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
