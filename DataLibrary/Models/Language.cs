namespace DataLibrary.Models;

public partial class Language
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<LocalizedEquipment> LocalizedEquipments { get; set; } = new List<LocalizedEquipment>();

    public virtual ICollection<LocalizedExercise> LocalizedExercises { get; set; } = new List<LocalizedExercise>();

    public virtual ICollection<LocalizedMuscleGroup> LocalizedMuscleGroups { get; set; } = new List<LocalizedMuscleGroup>();

    public virtual ICollection<LocalizedMuscle> LocalizedMuscles { get; set; } = new List<LocalizedMuscle>();

    public virtual ICollection<TrainingType> TrainingTypes { get; set; } = new List<TrainingType>();
}
