namespace DataLibrary;

//represets a training session no matter how big or small
// 60, 12, "GO SLOWER", 2024-06-21 17:26:24
public class TrainingSession
{
    public int Id { get; set; }
    public int DurationInSec { get; set; }
    public int Calories { get; set; }
    public string? Notes { get; set; }
    public int? MesoCycle { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TrainingSessionType> Types { get; set; }
    public List<TrainingSessionExerciseRecord> ExerciseRecords { get; set; }
}
