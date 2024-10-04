namespace SharedLibrary.Dtos;
public class MuscleReadDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Function { get; set; }
    public string? WikiPageUrl { get; set; }
}

public class MuscleWriteDto
{
    public required string Name { get; set; }
    public required string MuscleGroup { get; set; }
    public required string Function { get; set; }
    public required string WikiPageUrl { get; set; }

}

public class MuscleUpdateDto
{
    public string? Name { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Function { get; set; }
    public string? WikiPageUrl { get; set; }

}

public class MuscleExerciseReadDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Function { get; set; }
    public string? WikiPageUrl { get; set; }
    public bool? IsPrimary { get; set; }
}