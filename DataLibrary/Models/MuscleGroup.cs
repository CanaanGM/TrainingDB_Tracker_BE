namespace DataLibrary.Models;

public partial class MuscleGroup
{
    public int Id { get; set; }

    public virtual ICollection<LocalizedMuscleGroup> LocalizedMuscleGroups { get; set; } = new List<LocalizedMuscleGroup>();
}
