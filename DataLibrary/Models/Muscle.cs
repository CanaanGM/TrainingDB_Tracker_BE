namespace DataLibrary.Models;

public partial class Muscle
{
    public int Id { get; set; }

    public virtual ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();

    public virtual ICollection<LocalizedMuscle> LocalizedMuscles { get; set; } = new List<LocalizedMuscle>();

    public virtual ICollection<MuscleGroup> MuscleGroups { get; set; } = new List<MuscleGroup>();
}
