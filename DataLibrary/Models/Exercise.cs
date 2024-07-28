namespace DataLibrary.Models;

public partial class Exercise
{
    public int Id { get; set; }

    public int? Difficulty { get; set; }

    public virtual ICollection<BlockExercise> BlockExercises { get; set; } = new List<BlockExercise>();

    public virtual ICollection<ExerciseHowTo> ExerciseHowTos { get; set; } = new List<ExerciseHowTo>();

    public virtual ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();

    public virtual ICollection<LocalizedEquipment> LocalizedEquipments { get; set; } = new List<LocalizedEquipment>();

    public virtual ICollection<LocalizedExercise> LocalizedExercises { get; set; } = new List<LocalizedExercise>();

    public virtual ICollection<UserExercise> UserExercises { get; set; } = new List<UserExercise>();

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<TrainingType> TrainingTypes { get; set; } = new List<TrainingType>();
}
