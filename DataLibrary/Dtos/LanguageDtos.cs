using System.ComponentModel.DataAnnotations;

namespace DataLibrary.Dtos;

public class LanguageWriteDto
{
    [Required]
    [Length(minimumLength: 2, maximumLength: 20)]
    public string Code { get; set; } = null!;

    [Required]
    [Length(minimumLength:3, maximumLength: 50)]
    public string Name { get; set; } = null!;
}

public class LanguageReadDto : LanguageWriteDto
{
    public int Id { get; set; }
}