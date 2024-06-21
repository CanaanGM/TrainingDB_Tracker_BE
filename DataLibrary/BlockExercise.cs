namespace DataLibrary;

public class BlockExercise
{
    public int Id { get; set; }
    public Exercise Exercise { get; set; }
    public int ExerciseId { get; set; }
    public Block Block { get; set; }
    public int BlockId { get; set; }
    public int OrderNumber { get; set; }
    public string Instructions { get; set; }
    public int Reps { get; set; }
    public int Timer { get; set; }
    public int Distance { get; set; }
}