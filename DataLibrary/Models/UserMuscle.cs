namespace DataLibrary.Models;

public partial class UserMuscle
{
    public int? UserId { get; set; }

    public int? MuscleGroupId { get; set; }

    public int? MuscleCooldown { get; set; }

    public int? Frequency { get; set; }

    public int? TrainingVolume { get; set; }

    public virtual MuscleGroup? MuscleGroup { get; set; }

    public virtual User? User { get; set; }
}
