namespace DataLibrary.Dtos;

public class TrainingPlanWriteDto
{
    public required string? Name { get; set; }
    public required string? Description { get; set; }
    public required string? Notes { get; set; }
    public required List<TrainingWeekWriteDto> TrainingWeeks { get; set; }
    public required List<string> Equipemnt { get; set; }
    public required List<string> TrainingTypes { get; set; }
}

public class TrainingWeekWriteDto
{
    public string Name { get; set; } = null!;
    public required int OrderNumber { get; set; }
    public required List<TrainingDaysWriteDto> TrainingDays { get; set; }
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
    public required List<BlockExerciseWriteDto> BlockExercises { get; set; }
}

public class BlockExerciseWriteDto
{
    public required string ExerciseName { get; set; }
    public required string Instructions { get; set; }
    public required int OrderNumber { get; set; }
    public int? Repetitions { get; set; }
    public int? TimerInSeconds { get; set; }
    public int? DistanceInMeters { get; set; }
}

public class TrainingPlanReadDto
{
    //TODO: make this more useful
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Notes { get; set; }
    public List<string> TrainingTypes { get; set; }
    public List<string> RequiredEquipment { get; set; }
    public List<TrainingWeekReadDto> TrainingWeeks { get; set; }
    
}

public class TrainingWeekReadDto
{
    public string Name { get; set; } = null!;

    public int OrderNumber { get; set; }
    public List<TrainingDaysReadDto> TrainingDays { get; set; }
}

public class TrainingDaysReadDto
{
    public string Name { get; set; } = null!;
    public string? Notes { get; set; }
    public int OrderNumber { get; set; }
    public List<BlockReadDto> Blocks { get; set; }
}

public class BlockReadDto
{
    public required string Name { get; set; } = null!;
    public required int Sets { get; set; }
    public required int RestInSeconds { get; set; }
    public required string Instructions { get; set; }
    public required int OrderNumber { get; set; }
    public required List<BlockExerciseReadDto> BlockExercises { get; set; }
}

public class BlockExerciseReadDto
{
    public ExerciseReadDto Exercise { get; set; }
    public required string Notes { get; set; }
    public required int OrderNumber { get; set; }
    public int? Repetitions { get; set; }
    public int? TimerInSeconds { get; set; }
    public int? DistanceInMeters { get; set; }
}