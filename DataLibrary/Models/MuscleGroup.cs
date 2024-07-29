namespace DataLibrary.Models;

public partial class MuscleGroup
{
    public int Id { get; set; }

    public virtual ICollection<LocalizedMuscleGroup> LocalizedMuscleGroups { get; set; } = new List<LocalizedMuscleGroup>();

    public virtual ICollection<Muscle> Muscles { get; set; } = new List<Muscle>();
}
