namespace DataLibrary;

// this is a singluar exercise record: PullUps, 20 reps, 0 weight, 0 timer, "Go Slower next time", 0 2024-06-21 17:17:33.
public class ExerciseRecord
{
    public int Id { get; set; }
    public Exercise Exercise { get; set; }
    public int ExerciseId { get; set; }
    public double Reps { get; set; }
    public int Timer { get; set; }
    public double Weight { get; set; }
    public string? Notes { get; set; }
    public double LastWeightUsed { get; set; } // latest weight used before this session
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}
