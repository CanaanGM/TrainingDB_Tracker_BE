namespace DataLibrary.Dtos;
public class ExerciseReadDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? HowTo { get; set; }

    public int? Difficulty { get; set; }

    public List<TrainingTypeReadDto> TrainingTypes { get; set; }
    public List<MuscleExerciseReadDto> ExerciseMuscles { get; set; }
    public List<ExerciseHowToReadDto> HowTos { get; set; }
}

public class ExerciseWriteDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? HowTo { get; set; }
    public int? Difficulty { get; set; }
    public List<ExerciseHowToWriteDto> HowTos { get; set; }
    public List<string> TrainingTypes { get; set; }
    public List<ExerciseMuscleWriteDto> ExerciseMuscles { get; set; }
}

public class ExerciseMuscleWriteDto
{
    public required string MuscleName { get; init; }
    public required bool IsPrimary { get; init; }
}


public class ExerciseHowToReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
}

public class ExerciseHowToWriteDto
{
    public required string Name { get; set; } = null!;
    public required string Url { get; set; } = null!;
}

public class ExerciseFilterDto
{
    public string? TrainingTypeName { get; set; }
    public string? MuscleName { get; set; }
    public string? MuscleGroupName { get; set; }
    public int? MinimumDifficulty { get; set; }
}
