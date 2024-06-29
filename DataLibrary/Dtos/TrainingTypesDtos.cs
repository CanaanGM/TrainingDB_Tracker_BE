namespace DataLibrary.Dtos;
public class TrainingTypeReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}


public class TrainingTypeWriteDto
{
    public required string Name { get; set; }
}
