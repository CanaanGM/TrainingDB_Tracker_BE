using System.ComponentModel.DataAnnotations;
using DataLibrary.Helpers;

namespace DataLibrary.Dtos;

public class TrainingTypeReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class TrainingTypeWriteDto
{
    private string _name { get; set; }
    private string _languageCode { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(64)]
    public required string Name
    {
        get => _name;
        set => _name = Utils.NormalizeString(value);
    }

    [Required]
    [MinLength(2)]
    [MaxLength(16)]
    public required string LanguageCode
    {
        get => _languageCode;
        set => _languageCode = Utils.NormalizeString(value);
    }
}

public class TrainingTypeUpdateDto : TrainingTypeWriteDto
{
    private string _newName;
    [MinLength(2)]
    [MaxLength(64)]
    public string NewName
    {
        get => _newName;
        set => _newName = Utils.NormalizeString(value);
    }
}