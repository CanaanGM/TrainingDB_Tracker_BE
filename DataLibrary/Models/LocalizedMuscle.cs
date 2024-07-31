namespace DataLibrary.Models;

public partial class LocalizedMuscle
{
    public int MuscleId { get; set; }

    public int LanguageId { get; set; }

    public string Name { get; set; } = null!;

    public string MuscleGroup { get; set; } = null!;

    public string? Function { get; set; }

    public string? WikiPageUrl { get; set; }

    public virtual Language Language { get; set; } = null!;

    public virtual Muscle Muscle { get; set; } = null!;
}
