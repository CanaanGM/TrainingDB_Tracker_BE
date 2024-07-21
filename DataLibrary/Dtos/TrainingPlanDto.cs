namespace DataLibrary.Dtos;

public class TrainingPlanWriteDto
{
    public string? Name { get; set; }
    public int TrainingWeekCount { get; set; }
    public int TrainingDaysPerWeek { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public List<TrainingWeekWriteDto> TrainingWeeks { get; set; }
    // public List<EquipmentWriteDto> RequiredEquipemnt { get; set; }
    public List<TrainingTypeWriteDto> TrainingTypes { get; set; }
}

public class TrainingWeekWriteDto
{
    public string Name { get; set; } = null!;
    public int OrderNumber { get; set; }
    private List<TrainingDaysWriteDto> Days { get; set; }
}

public class TrainingDaysWriteDto
{
    public string Name { get; set; } = null!;
    public string? Notes { get; set; }
    public int? OrderNumber { get; set; }
    private List<BlockWriteDto> Block { get; set; }
}

public class BlockWriteDto
{
    public string Name { get; set; } = null!;

    public int? Sets { get; set; }

    public int? RestInSeconds { get; set; }

    public string? Instrcustions { get; set; }

    public int? OrderNumber { get; set; }
    private List<BlockExerciseWriteDto> Exercises { get; set; }
}

public class BlockExerciseWriteDto
{
    public string ExerciseName { get; set; }
    public int? OrderNumber { get; set; }
    public string? Instructions { get; set; }
    public int? Repetitions { get; set; }
    public int? TimerInSeconds { get; set; }
    public int? DistanceInMeters { get; set; }
}