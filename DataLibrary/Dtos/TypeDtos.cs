namespace DataLibrary.Dtos;
// for anything that uses a type
public class TypeReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class TypeWriteDto
{
    public required string Name { get; init; }
}