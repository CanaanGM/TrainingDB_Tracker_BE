using System.ComponentModel.DataAnnotations;
using DataLibrary.Helpers;

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
    public List<EquipmentWriteDto> EquipmentNeeded { get; set; }
}

public class ExerciseSearchResultDto
{
    private string _name;

    [Required]
    public string Name
    {
        get => _name;
        set => _name = Utils.NormalizeString(value);
    }
}

public class ExerciseWriteDto
{
    private string _name;
    private int _difficulty;
    private string _languageCode;
    private List<string> _equipmentNeeded;
    private List<string> _trainingTypes;
    [Required]
    public string Name
    {
        get => _name;
        set => _name = Utils.NormalizeString(value);
    }

    [Required]
    public string LanguageCode
    {
        get => _languageCode;
        set => _languageCode = Utils.NormalizeString(value);
    }
    
    public string? Description { get; set; }
    public string? HowTo { get; set; }

    public int? Difficulty
    {
        get => _difficulty;
        set
        {
            if (value <= 0) _difficulty = 1;
            if (value >= 5) _difficulty = 5;
            else _difficulty = (int) value!;
        }
    }

    public List<ExerciseHowToWriteDto> HowTos { get; set; }
    public List<string> TrainingTypes { get => _trainingTypes; set => _trainingTypes = Utils.NormalizeStringList(value); }
    public List<ExerciseMuscleWriteDto> ExerciseMuscles { get; set; }
    public List<string> EquipmentNeeded { get => _equipmentNeeded; set =>_equipmentNeeded = Utils.NormalizeStringList(value) ; }

}

public class ExerciseMuscleWriteDto
{
    private string _muscleName;

    public string MuscleName
    {
        get => _muscleName;
        init => _muscleName = Utils.NormalizeString(value);
    }
    public bool? IsPrimary { get; init; } = false;
}

public class ExerciseHowToReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
}

public class ExerciseHowToWriteDto
 {
    private string _name;

    [Required]
    public string Name
    {
        get => _name;
        set => _name = Utils.NormalizeString(value);
    }

    [Required] public string Url { get; set; } = null!;
}

public class ExerciseFilterDto
{
    public string? TrainingTypeName { get; set; }
    public string? MuscleName { get; set; }
    public string? MuscleGroupName { get; set; }
    public int? MinimumDifficulty { get; set; }
}