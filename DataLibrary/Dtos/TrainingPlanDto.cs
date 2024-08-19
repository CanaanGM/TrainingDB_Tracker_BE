using DataLibrary.Helpers;

namespace DataLibrary.Dtos;

public class TrainingPlanWriteDto
{
    public string _name { get; set; }
    public required string Name { get => _name; set => _name = Utils.NormalizeString(value); }
    public required string? Description { get; set; }
    public required string? Notes { get; set; }
    public required List<TrainingWeekWriteDto> TrainingWeeks { get; set; }
}

public class TrainingWeekWriteDto
{
    public string _name { get; set; }
    public required string Name { get => _name; set => _name = Utils.NormalizeString(value); }
    public required int OrderNumber { get; set; }
    public required List<TrainingDaysWriteDto> TrainingDays { get; set; }
}

public class TrainingDaysWriteDto
{
    public string _name { get; set; }
    public required string Name { get => _name; set => _name = Utils.NormalizeString(value); }
    public string? Notes { get; set; }
    public required int OrderNumber { get; set; }
    public List<BlockWriteDto> Blocks { get; set; }
}

public class BlockWriteDto
{
    public string _name { get; set; }
    public required string Name { get => _name; set => _name = Utils.NormalizeString(value); }
    public required int Sets { get; set; }
    public required int RestInSeconds { get; set; }
    public required string Instructions { get; set; }
    public required int OrderNumber { get; set; }
    public required List<BlockExerciseWriteDto> BlockExercises { get; set; }
}

public class BlockExerciseWriteDto
{
    public string _name { get; set; }
    public required string ExerciseName { get => _name; set => _name = Utils.NormalizeString(value); }
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
    /// <summary>
    /// from the exercises
    /// </summary>
    public List<string> TrainingTypes { get; set; }
    /// <summary>
    /// From the exercises
    /// </summary>
    public List<string> RequiredEquipment { get; set; }
    public List<TrainingWeekReadDto> TrainingWeeks { get; set; }
    public DateTime CreatedAt { get; set; }
    
}

public class TrainingWeekReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int OrderNumber { get; set; }
    public List<TrainingDaysReadDto> TrainingDays { get; set; }
}

public class TrainingDaysReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Notes { get; set; }
    public int OrderNumber { get; set; }
    public List<BlockReadDto> Blocks { get; set; }
}

public class BlockReadDto
{
    public int Id { get; set; }
    public required string Name { get; set; } = null!;
    public required int Sets { get; set; }
    public required int RestInSeconds { get; set; }
    public required string Instructions { get; set; }
    public required int OrderNumber { get; set; }
    public required List<BlockExerciseReadDto> BlockExercises { get; set; }
}

public class BlockExerciseReadDto
{
    public int Id { get; set; }
    public ExerciseReadDto Exercise { get; set; }
    public required string Notes { get; set; }
    public required int OrderNumber { get; set; }
    public int? Repetitions { get; set; }
    public int? TimerInSeconds { get; set; }
    public int? DistanceInMeters { get; set; }
}