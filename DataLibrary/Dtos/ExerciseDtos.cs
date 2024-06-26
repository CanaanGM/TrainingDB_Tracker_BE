namespace DataLibrary.Dtos;
public class ExerciseReadDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<MuscleReadDto> ExerciseMuscles { get; set; } = new List<MuscleReadDto>();
    public virtual ICollection<TypeReadDto> ExerciseTypes { get; set; } = new List<TypeReadDto>();
}


public class ExerciseWriteDto
{
    // muscles groups: should already be in the database, only create the howtos
    // ex: DragonFlag, Insane core exercise, [{"youtube", "https://utube.com"}],["Abs"]
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<ExerciseHowToDto> HowToLinks { get; init; }
    public required List<string> MuscleGroups { get; init; }
    public required List<string> Types { get; init; }

}


public class ExerciseHowToDto
{
    public required string Name { get; init; }
    public required string Url { get; init; }
}