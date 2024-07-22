namespace DataLibrary.Dtos;

public class TrainingPlanWriteDto
{
    public required string? Name { get; set; }
    public required string? Description { get; set; }
    public required string? Notes { get; set; }
    public required List<TrainingWeekWriteDto> Weeks { get; set; }
    public required List<string> Equipemnt { get; set; }
    public required List<string> TrainingTypes { get; set; }
}

public class TrainingWeekWriteDto
{
    public string Name { get; set; } = null!;
    public required int OrderNumber { get; set; }
    public  required List<TrainingDaysWriteDto> Days { get; set; }
}

public class TrainingDaysWriteDto
{
    public required string Name { get; set; } = null!;
    public string? Notes { get; set; }
    public required int OrderNumber { get; set; }
    public List<BlockWriteDto> Blocks { get; set; }
}

public class BlockWriteDto
{
    public required string Name { get; set; } = null!;
    public required int Sets { get; set; }
    public required int RestInSeconds { get; set; }
    public required string Instructions { get; set; }
    public required int OrderNumber { get; set; }
    public required List<BlockExerciseWriteDto> Exercises { get; set; }
}

public class BlockExerciseWriteDto
{
    public required string ExerciseName { get; set; }
    public required string Notes { get; set; }
    public required int OrderNumber { get; set; }
    public int? Repetitions { get; set; }
    public int? TimerInSeconds { get; set; }
    public int? DistanceInMeters { get; set; }
}