namespace DataLibrary.Models;

public partial class LocalizedMuscleGroup
{
    public int MuscleGroup { get; set; }

    public int LanguageId { get; set; }

    public string Name { get; set; } = null!;

    public string? Function { get; set; }

    public string? WikiPageUrl { get; set; }

    public virtual Language Language { get; set; } = null!;

    public virtual MuscleGroup MuscleGroupNavigation { get; set; } = null!;
}
