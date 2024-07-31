using System.ComponentModel.DataAnnotations;
using DataLibrary.Helpers;

namespace DataLibrary.Dtos;

public class MuscleReadDto
{
    public int Id { get; set; }
    public string? MuscleName { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Function { get; set; }
    public string? WikiPageUrl { get; set; }
    public string? LanguageName { get; set; }
}

public class MuscleWriteDto
{
    private string _name;
    private string _languageCode;
    private string _muscleGroup;

    [Required]
    public string Name
    {
        get => _name;
        init => _name = Utils.NormalizeString(value);
    }

    [Required] public string MuscleGroup
    {
        get => _muscleGroup;
        init => _muscleGroup =  Utils.NormalizeString(value);
    }

    [Required] public string Function { get; set; }

    [Required]
    public string LanguageCode
    {
        get => _languageCode;
        set => _languageCode = Utils.NormalizeString(value);
    }

    public string WikiPageUrl { get; set; } = string.Empty;
}

public class MuscleUpdateDto : MuscleWriteDto
{
    // maybe later 
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